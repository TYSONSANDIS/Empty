﻿/**
 * AssetBundle资源管理
 */

using Base.Common;
using UnityEngine;
using Base.Debug;
using Base.Pool;
using System.IO;
using System.Collections.Generic;
using Base.Utils;
using System.Runtime.Serialization.Formatters.Binary;

public class AssetBundleManager : Singleton<AssetBundleManager>
{
    /// <summary>
    /// 配置文件路径
    /// </summary>
    private readonly string ASSETBUNDLECONFIGPATH = Application.streamingAssetsPath + "/assetbundleconfig";
    /// <summary>
    /// 以md5为key存储一份AssetBundleBase
    /// </summary>
    private Dictionary<string, AssetBundleBase> mAssetBundleBaseDict = new Dictionary<string, AssetBundleBase>();
    /// <summary>
    /// 存储加载的AssetBundleItem
    /// </summary>
    private Dictionary<string, AssetBundleItem> mAssetBundleItemDict = new Dictionary<string, AssetBundleItem>();
    /// <summary>
    /// AssetBundleItem对应的类对象池
    /// </summary>
    private ClassObjectPool<AssetBundleItem> mAssetBundleItemPool = ObjectManager.Instance.GetOrCreateClassPool<AssetBundleItem>();

    /// <summary>
    /// 加载AssetBundle配置文件
    /// </summary>
    public bool LoadAssetBundleConfig()
    {
        mAssetBundleBaseDict.Clear();
        mAssetBundleItemDict.Clear();
        var assetBundle = AssetBundle.LoadFromFile(ASSETBUNDLECONFIGPATH);
        var textAsset = assetBundle.LoadAsset<TextAsset>("assetbundleconfig");
        if (textAsset == null)
        {
            Debugger.LogError("AssetBundleConfig is not exist!");
            return false;
        }
        var stream = new MemoryStream(textAsset.bytes);
        var formatter = new BinaryFormatter();
        var assetBundleConfig = (AssetBundleConfig)formatter.Deserialize(stream);
        stream.Close();
        for (var i = 0; i < assetBundleConfig.AssetBundleList.Count; i++)
        {
            var abBase = assetBundleConfig.AssetBundleList[i];
            var md5 = abBase.MD5;
            if (mAssetBundleBaseDict.ContainsKey(md5))
            {
                Debugger.LogError("Duplicate MD5! AssetName:{0}, ABName:{1}", abBase.AssetName, abBase.ABName);
            }
            else
            {
                mAssetBundleBaseDict.Add(md5, abBase);
            }
        }
        return true;
    }

    /// <summary>
    /// 获取ABBase资源
    /// </summary>
    public AssetBundleBase GetAssetBundleBase(string md5)
    {
        if (mAssetBundleBaseDict.ContainsKey(md5))
        {
            return mAssetBundleBaseDict[md5];
        }
        return null;
    }

    /// <summary>
    /// 根据AB包名加载AssetBundle
    /// </summary>
    public AssetBundle LoadAssetBundle(string name)
    {
        AssetBundleItem item = null;
        if (!mAssetBundleItemDict.TryGetValue(name, out item))
        {
            AssetBundle assetBundle = null;
            var path = StringUtil.Concat(Application.streamingAssetsPath, "/", name);
            if (File.Exists(path)) assetBundle = AssetBundle.LoadFromFile(path);
            if (assetBundle == null) Debugger.LogError("Load AssetBundle Error: " + name);
            item = mAssetBundleItemPool.Spawn();
            item.AssetBundle = assetBundle;
            item.RefCount++;
            mAssetBundleItemDict.Add(name, item);
        }
        else
        {
            item.RefCount++;
        }
        return item.AssetBundle;
    }

    /// <summary>
    /// 加载AssetBundleBase所依赖的所有AB包
    /// </summary>
    public AssetBundle LoadAssetBundle(AssetBundleBase abBase)
    {
        if (abBase == null) return null;
        var ab = LoadAssetBundle(abBase.ABName);
        if (abBase.ABDependList != null)
        {
            for (var i = 0; i < abBase.ABDependList.Count; i++)
            {
                LoadAssetBundle(abBase.ABDependList[i]);
            }
        }
        return ab;
    }

    /// <summary>
    /// 卸载AssetBundle
    /// </summary>
    public void UnloadAssetBundle(string name)
    {
        AssetBundleItem item = null;
        if (mAssetBundleItemDict.TryGetValue(name, out item) && item != null)
        {
            item.RefCount--;
            if (item.RefCount <= 0 && item.AssetBundle != null)
            {
                item.AssetBundle.Unload(true);
                item.Reset();
                mAssetBundleItemPool.Recycle(item);
                mAssetBundleItemDict.Remove(name);
            }
        }
    }
}

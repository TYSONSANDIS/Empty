﻿/**
 * 热更新资源下载器
 */

using UnityEngine;
using Base.Debug;
using Base.Utils;
using System.Collections;

public class ResourceUpdater : MonoBehaviour
{
    private const string VERSIONCONFIGFILE = "VersionConfig.json";
    /// <summary>
    /// 本地版本配置信息
    /// </summary>
    private VersionConfig mLocalVersionConfig;
    /// <summary>
    /// 服务器版本配置信息
    /// </summary>
    private VersionConfig mServerVersionConfig;

    /// <summary>
    /// 开始更新
    /// </summary>
    private void Start()
    {
        StartCoroutine(CheckUpdate());
    }

    /// <summary>
    /// 检查更新，包括大版本换包和热更新
    /// </summary>
    private IEnumerator CheckUpdate()
    {
        // 是否开启热更检测
        if (!AppConfig.CheckVersionUpdate)
        {
            StartGame();
            yield break;
        }
        // 加载并初始化版本信息文件
        yield return InitVersion();
        // 检测版本配置文件
        if (CheckVersion(mLocalVersionConfig.Version, mServerVersionConfig.Version))
        {
            // 需要热更新时，对比版本资源
            //UpdateVersionConfig();
            yield return CompareVersion();
            yield break;
        }
        StartGame();
    }

    /// <summary>
    /// 初始化本地版本
    /// </summary>
    IEnumerator InitVersion()
    {
        var localVersionConfigPath = PathUtil.GetLocalFilePath(VERSIONCONFIGFILE);
        var www = new WWW(localVersionConfigPath);
        yield return www;
        mLocalVersionConfig = JsonUtility.FromJson<VersionConfig>(www.text);
        www.Dispose();
        var serverVersionConfigPath = PathUtil.GetServerFileURL(VERSIONCONFIGFILE);
        www = new WWW(serverVersionConfigPath);
        yield return www;
        mServerVersionConfig = string.IsNullOrEmpty(www.error) ? JsonUtility.FromJson<VersionConfig>(www.text) : mLocalVersionConfig;
        www.Dispose();
    }

    /// <summary>
    /// 更新配置文件
    /// </summary>
    private void UpdateVersionConfig()
    {
        var path = PathUtil.GetPresistentDataFilePath(VERSIONCONFIGFILE);
        var text = JsonUtility.ToJson(mServerVersionConfig);
        FileUtil.WriteAllText(path, text);
    }
    
    /// <summary>
    /// 版本对比
    /// </summary>
    private bool CheckVersion(string sourceVersion, string targetVersion)
    {
        string[] sourceVers = sourceVersion.Split('.');
        string[] targetVers = targetVersion.Split('.');
        try
        {
            int sV0 = int.Parse(sourceVers[0]);
            int tV0 = int.Parse(targetVers[0]);
            int sV1 = int.Parse(sourceVers[1]);
            int tV1 = int.Parse(targetVers[1]);
            // 大版本更新
            if (tV0 > sV0)
            {
                Debugger.Log("New Version");
                return false;
            }
            // 热更新
            if (tV0 == sV0 && tV1 > sV1)
            {
                Debugger.Log("Update Res ...");
                return true;
            }
        }
        catch (System.Exception e)
        {
            Debugger.LogError(e.Message);
        }
        return false;
    }

    /// <summary>
    /// 比较版本资源
    /// </summary>
    /// <returns></returns>
    IEnumerator CompareVersion()
    {
        var manifestAssetBundlePath = StringUtil.PathConcat(AssetBundleConfig.AssetBundlesFolder, AssetBundleConfig.AssetBundlesFolder);
        // 本地的AssetBundleManifest
        var localManifestPath = PathUtil.GetLocalFilePath(manifestAssetBundlePath);
        var www = new WWW(localManifestPath);
        yield return www;
        var localManifest = www.assetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        www.Dispose();
        //var localAllAssetBundles = localManifest.GetAllAssetBundles();
        var serverManifestPath = PathUtil.GetServerFileURL(manifestAssetBundlePath);
        www = new WWW(serverManifestPath);
        yield return www;
        var serverManifest = www.assetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        www.Dispose();
    }

    /// <summary>
    /// 正式开始游戏
    /// </summary>
    private void StartGame()
    {
        ScenesManager.Instance.LoadSceneSync("Main");
    }

    private void DownLoadResource()
    {
    }

    /// <summary>
    /// 下载资源
    /// </summary>
    IEnumerator DownLoad(string url, System.Action<WWW> callback)
    {
        yield return null;
    }
}

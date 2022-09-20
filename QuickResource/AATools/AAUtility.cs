using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class AAUtility {
    public static string GetAddress(this AssetReference reference) {
        var loadResourceLocations = Addressables.LoadResourceLocationsAsync(reference);
        var result = loadResourceLocations.WaitForCompletion();
        string key = string.Empty;
        if (result.Count > 0) {
            key = result[0].PrimaryKey;
        }
        Addressables.Release(loadResourceLocations);
        return key;
    }

    public static AsyncOperationHandle<T> LoadAA<T>(string assetName, Action<AsyncOperationHandle<T>> callBack) {
        AsyncOperationHandle<T> output = Addressables.LoadAssetAsync<T>(assetName);
        output.Completed += callBack;
        return output;
    }
}

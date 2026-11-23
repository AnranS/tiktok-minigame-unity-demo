mergeInto(LibraryManager.library, {
    StarkAbfsCheckReady: function() {
        if (!window.StarkSDK || !window.StarkSDK.abfs)
            return false;
        return window.StarkSDK.abfs.checkReady();
    },
    StarkAbfsRegisterAssetBundleUrl: function(ptr) {
        if (!window.StarkSDK || !window.StarkSDK.abfs) return;
        var url = UTF8ToString(ptr);
        window.StarkSDK.abfs.registerAssetBundleUrl(url);
    },
    StarkAbfsIsAssetBundleUrlRegistered: function(ptr) {
        if (!window.StarkSDK || !window.StarkSDK.abfs)
            return false;
        var url = UTF8ToString(ptr);
        return window.StarkSDK.abfs.isRegisteredAssetBundlePath(url);
    },
    StarkAbfsUnregisterAssetBundleUrl: function(ptr) {
        if (!window.StarkSDK || !window.StarkSDK.abfs) return;
        var url = UTF8ToString(ptr);
        window.StarkSDK.abfs.unregisterAssetBundleUrl(url);
    }
});

module.exports = {
	originalWasmMd5: "$ORIGINALWASMMD5",
    wasmTableSize: "$WASMTABLESIZE",
    wasmSplitVersion: "$WASMSPLITVERSION",
    subJsUrl: "$SUBJSURL",
    usingWasmH5: "$USINGWASMH5", 
    enableWasmCollect: "$ENABLEWASMCOLLECT",
    enableWasmSplit: "$ENABLEWASMSPLIT",
    globalVarList: ["$GLOBALVARLIST"],
    wasmCollectAPIVer: "v4",

    ANDROID_CODE_FILE_MD5: "$ANDROID_CODE_FILE_MD5",
    ANDROID_SUB_CODE_FILE_MD5: "$SUB_CODE_FILE_MD5", 
    ARCHIVE_CODE_FILE_MD5: "$ARCHIVE_CODE_FILE_MD5", 
    IOS_CODE_FILE_MD5: "$IOS_CODE_FILE_MD5",
    enableArchiveMode: "$ENABLEARCHIVEMODE",
    // Compatibility field written by older split CLIs. The plugin now starts the
    // archive fetch in parallel for every archive product (main still waits for
    // the parsed archive), so this flag no longer changes loading behaviour.
    archiveBootClosureReady: false
}

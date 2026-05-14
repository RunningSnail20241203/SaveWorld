// Fix: SDK v0.1.32 missing WX_SyncFunction_tnnt symbol
// tnnt = return string, params: (string, number, number)
// This file supplements the SDK's SDK-Call-JS.jslib which lacks this variant.
// Provides a fallback implementation that returns empty string if WXWASMSDK is unavailable.
mergeInto(LibraryManager.library, {
WX_SyncFunction_tnnt: function(functionName, returnType, param1, param2){
    var res = '';
    if (typeof window !== 'undefined' && window.WXWASMSDK && typeof window.WXWASMSDK.WX_SyncFunction_tnnt === 'function') {
        res = window.WXWASMSDK.WX_SyncFunction_tnnt(_WXPointer_stringify_adaptor(functionName), _WXPointer_stringify_adaptor(returnType), param1, param2);
    }
    var bufferSize = lengthBytesUTF8(res || '') + 1;
    var buffer = _malloc(bufferSize);
    stringToUTF8((res || ''), buffer, bufferSize);
    return buffer;
},
});

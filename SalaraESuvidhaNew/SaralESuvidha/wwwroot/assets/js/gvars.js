//var loaderImg = '<img src="/assets/img/Infinity-0.8s-21px.gif" alt="working" />';
//var loaderImgGreen = '<img src="/assets/img/InfinityGreen-0.8s-21px.gif" alt="working" />';

function a2hex(str) {
    var arr = [];
    for (var i = 0, l = str.length; i < l; i++) {
        var hex = Number(str.charCodeAt(i)).toString(16);
        arr.push(hex);
    }
    return arr.join('');
}
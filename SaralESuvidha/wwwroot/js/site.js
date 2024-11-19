// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function ColorCodedValue(data){
    if (data === 'DISAPPROVED' || data === 'FAILURE' || data === 'FAILED') {
        return "<span class='text-danger'>" + data + "</span>";
    }
    else if (data === 'APPROVED' || data === 'SUCCESS') {
        return "<span class='text-success'>" + data + "</span>";
    }
    else if (data === 'PROCESS' || data === 'PENDING') {
        return "<span class='text-primary'>" + data + "</span>";
    }
    else if (data === 'REFUND') {
        return "<span class='text-facebook'>" + data + "</span>";
    }
    else
    {
        return data;
    }
}

function ConvertFormToJSON(form) {
    var array = jQuery(form).serializeArray();
    var json = {};

    jQuery.each(array, function () {
        json[this.name] = this.value || '';
    });

    return json;
}

function ConvertFormToJSONMultiselect(form) {
    var array = jQuery(form).serializeArray();
    var json = {};

    jQuery.each(array, function () {
        if (typeof json[this.name] == 'undefined') {
            json[this.name] = this.value || '';
        }
        else {
            json[this.name] += "," + this.value;
        }
    });

    return json;
}

String.prototype.convertToHex = function (delim) {
    return this.split("").map(function (c) {
        return ("0" + c.charCodeAt(0).toString(16)).slice(-2);
    }).join(delim || "");
};

function ascii2hex(str) {
    var hex = '';
    for (var i = 0, l = str.length; i < l; i++) {
        var hexx = Number(str.charCodeAt(i)).toString(16);
        hex += (hexx.length > 1 && hexx || '0' + hexx);
    }
    return hex;
}

function HexToAscii(str) {
    hexString = str;
    strOut = '';
    for (x = 0; x < hexString.length; x += 2) {
        strOut += String.fromCharCode(parseInt(hexString.substr(x, 2), 16));
    }
    return strOut;
}

function a2hex(str) {
    var arr = [];
    for (var i = 0, l = str.length; i < l; i++) {
        var hex = Number(str.charCodeAt(i)).toString(16);
        arr.push(hex);
    }
    return arr.join('');
}

function redirect(url) {
    var ua = navigator.userAgent.toLowerCase(),
        isIE = ua.indexOf('msie') !== -1,
        version = parseInt(ua.substr(4, 2), 10);

    // Internet Explorer 8 and lower
    if (isIE && version < 9) {
        var link = document.createElement('a');
        link.href = url;
        document.body.appendChild(link);
        link.click();
    }

    // All other browsers can use the standard window.location.href (they don't lose HTTP_REFERER like Internet Explorer 8 & lower does)
    else {
        window.location.href = url;
    }
}

function Notify(subtitle, description, icon, icon_color, timeout=5000){
    $('#notification_subtitle').html(subtitle);
    $('#notification_description').html(description);
    $('.notification_icon').attr('name',icon);
    $('#notification_icon_color').removeClass().addClass('icon-box').addClass(icon_color);
    notification('notification_a',timeout);
}

function Preloader(start){
    if(start===1){
        $('#PreLoader').addClass('spinner-border').addClass('text-primary');
    }else{
        $('#PreLoader').removeClass();
    }
}

function CRSymbol(RStatus){
    if(RStatus==='SUCCESS' || RStatus==='FAILURE' || RStatus==='PROCESS' || RStatus==='INITIAL' || RStatus==='REFUND'){
        return '-';
    }else{
        return '+';
    }
}

function StatusColor(RStatus){
    if(RStatus==='SUCCESS' || RStatus==='RCREDIT' || RStatus==='FCREDIT'){
        return 'success';
    }else if(RStatus==='FAILURE' || RStatus==='REFUND'){
        return 'danger';
    }else{
        return 'primary';
    }
}

$(function () {
    $('.flatpickr-date').flatpickr({
        altInput: true,
        dateFormat: "Y-m-d",
        altFormat: "d-m-y",
        defaultDate: "today"
    });
    
});



var loaderImg = '<img src="assets/img/Infinity-0.8s-21px.gif" alt="working" />';
var loaderImgGreen = '<img src="assets/img/InfinityGreen-0.8s-21px.gif" alt="working" />';

var loaderImgInner = '<img src="/assets/img/Infinity-0.8s-21px.gif" alt="working" />';
var loaderImgGreenInner = '<img src="/assets/img/InfinityGreen-0.8s-21px.gif" alt="working" />';

// Auto update layout
if (window.layoutHelpers) {
    window.layoutHelpers.setAutoUpdate(true);
}

$(function () {
    // Initialize sidenav
    $('#layout-sidenav').each(function () {
        new SideNav(this, {
            orientation: $(this).hasClass('sidenav-horizontal') ? 'horizontal' : 'vertical'
        });
    });

    // Initialize sidenav togglers
    $('body').on('click', '.layout-sidenav-toggle', function (e) {
        e.preventDefault();
        window.layoutHelpers.toggleCollapsed();
    });

    // Swap dropdown menus in RTL mode
    if ($('html').attr('dir') === 'rtl') {
        $('#layout-navbar .dropdown-menu').toggleClass('dropdown-menu-right');
    }

    $('.selectpicker').selectpicker();

    $('.flatpickr-date').flatpickr({
        altInput: true,
        dateFormat: "Y-m-d",
        defaultDate: "today"
    });

});


var MERCY_Token = 'MERCY-Token';
var MERCY_Token_Value = '';
var MERCY_Token_Url_Param = '.t';

var MERCY_C_Token = '__amercy-t';

var MERCY_C_UI = '__amercy-ui';
var MERCY_UI_Value = '';
var MERCY_UI_Url_Param = '.ui';

var api_Request = '';
var ajax_LoginRequest = '';
var paramid = '';
var paramCheck = '';
var uInfo;
var checkToken_Is_Exists = true; // by default, always check value of Token

var current_Data_Form = {};
var permission = {};
var get_user_menu = '0';
var get_user_relation = '0';

var MERCY_C_HIDE = '__amercy-hi';

var getUrlParameter = function getUrlParameter(sParam) {
    var sPageURL = window.location.search.substring(1),
        sURLVariables = sPageURL.split('&'),
        sParameterName,
        i;

    for (i = 0; i < sURLVariables.length; i++) {
        sParameterName = sURLVariables[i].split('=');

        if (sParameterName[0] === sParam) {
            return sParameterName[1] === undefined ? true : decodeURIComponent(sParameterName[1]);
        }
    }
};

function mercyUrl(p_url) {
    // Token is not stored in Url anymore
    // 
    // Use Cookie in UI {User  Interface} only.
    // Token will be stored in Cookie.
    // Access to API/Service is still using Token within HTTP_Header.
    return p_url;// + '?'+MERCY_Token_Url_Param+'='; //+MERCY_Token_Value;
}

function successLogin(p_url, p_userInterface) {
    MERCY_Token_Value = '';

    try {
        MERCY_Token_Value = ajax_LoginRequest.getResponseHeader(MERCY_Token);
    }
    catch (err) { }

    // Save to Cookie
    $.cookie(MERCY_C_Token, MERCY_Token_Value);
    $.cookie(MERCY_C_UI, p_userInterface);

    window.location = mercyUrl(p_url);
}

function mercy_init() {
    MERCY_Token_Value = getUrlParameter(MERCY_Token_Url_Param);
    // If there is no value in Url
    if (MERCY_Token_Value == '' || MERCY_Token_Value == null) {
        // Read from Cookie
        MERCY_Token_Value = $.cookie(MERCY_C_Token);
    }

    MERCY_UI_Value = getUrlParameter(MERCY_UI_Url_Param);
    // If there is no value in Url
    if (MERCY_UI_Value == '' || MERCY_UI_Value == null) {
        // Read from Cookie
        MERCY_UI_Value = $.cookie(MERCY_C_UI);
    }

    paramid = getUrlParameter('.id');
    paramCheck = getUrlParameter('check');

    if (checkToken_Is_Exists) {
        if (MERCY_Token_Value == '' || MERCY_Token_Value == null) {
            window.location.href = '/?returnUrl=' + encodeURIComponent(window.location.href.replace('logout=1', ''));
        }
    }
}

function cleanURL() {
    try {
        var uri = window.location.toString();
        if (uri.indexOf("?") > 0) {
            var clean_uri = uri.substring(0, uri.indexOf("?"));
            window.history.replaceState({}, document.title, clean_uri);
        }
    }
    catch (err) { }
}

function logout_old() {
    $('#imgLoading').show();

    // Clear Cookie
    $.removeCookie(MERCY_C_Token);
    $.removeCookie(MERCY_C_UI);
    $.removeCookie(MERCY_C_HIDE);

    ajax_LoginRequest = $.ajax({
        url: api_Request + '/Api/User/Logout',
        type: 'GET',
        beforeSend: function (request) {
            request.setRequestHeader(MERCY_Token, MERCY_Token_Value);
        },
        cache: false,
        success: function (data) {
            $('#imgLoading').hide();

            window.location = '/';
        },
        error: function (error) {
            //$(this).remove();
        }
    });
}

function logout() {
    // Clear Cookie
    $.removeCookie(MERCY_C_Token);
    $.removeCookie(MERCY_C_UI);

    //clear localstorage
    localStorage.clear();

    ajax_LoginRequest = $.ajax({
        url: api_Request + '/Api/User/Logout',
        type: 'GET',
        beforeSend: function (request) {
            request.setRequestHeader(MERCY_Token, MERCY_Token_Value);
        },
        cache: false,
        success: function (data) {
        },
        error: function (error) {
            //$(this).remove();
        }
    });
}

function userInfo() {
    $.ajax({
        url: api_Request + '/Api/User/Info',
        type: 'GET',
        dataType: "json",
        beforeSend: function (request) {
            request.setRequestHeader(MERCY_Token, MERCY_Token_Value);
        },
        cache: false,
        success: function (data) {
            after_GetUserInfo(data);

            uInfo = data;
        },
        error: function (error) {
            //$(this).remove();
        }
    });
}

function getNotification_All() {
    $.ajax({
        //url: api_Request + '/Api/Notification/Get?type=Analysis'
        //url: api_Request + '/Api/Notification/Get?type=Sampling'
        url: api_Request + '/Api/Notification/GetAll'
        , type: 'GET'
        , dataType: "json"
        , beforeSend: function (request) {
            request.setRequestHeader(MERCY_Token, MERCY_Token_Value);
        }
        , cache: false
        , success: function (data) {
            if (!$.trim(data)) {
                // empty data
            }
            else {
                var name = $('#uName').html();
                var i = name.indexOf('#');
                if (i < 0) {
                    name = name + ' # ' + data.Count;
                }
                else {
                    name = name.substring(0, i) + ' # ' + data.Count;
                }

                $('#uName').html(name);
            }
        },
        error: function (error) {
            $(this).remove();
        }
    });
}

var user_biasa = false;
var user_lab = false;
var user_cpl = false;
var user_masterdata = false;
var user_consumable = false;
var user_labmaintenance = false;
var user_admin = false;

function showMenu2() {
    $('.mercy_menu_biasa').hide();
    $('.mercy_menu_cpl').hide();
    $('.mercy_menu_master').hide();
    $('.mercy_menu_user_lab').hide();
    $('.mercy_menu_consumable').hide();
    $('.mercy_menu_lab_mtn').hide();

    if (user_biasa || user_lab) {
        $('.mercy_menu_biasa').show();
    }
    if (user_cpl) {
        $('.mercy_menu_cpl').show();
    }
    if (user_masterdata) {
        $('.mercy_menu_master').show();
    }
    if (user_lab) {
        $('.mercy_menu_user_lab').show();
    }
    if (user_admin) {
        $('.mercy_menu_admin').show();
    }
    if (user_consumable) {
        $('.mercy_menu_consumable').show();
    }
    if (user_labmaintenance) {
        $('.mercy_menu_lab_mtn').show();
    }
}

function after_GetUserInfo(p_user) {
    $('#uName').html(p_user.Name);
    $('#uTitle').html(p_user.Title);

    showMenu(p_user);

    $('#uPicture').css("background-image", "url(/api/user/picture/" + p_user.UserId + ")");
    
    // set "Visibility" to User
    $('#loading').hide();
    $('#mercy_main_canvas').show();
    
    if ($.cookie(MERCY_C_HIDE) == '1'){
        rememberSidebar();
    }
}

function ValueBit(p_id) {
    var result = 0;

    if (p_id.indexOf('#') !== -1) {
        // that's ok, p_id already has '#'
    } else {
        // let's give '#'
        p_id = '#' + p_id;
    }

    if ($(p_id).is(':checked')) result = 1;

    return result;
}

function Is_ParamId_Blank() {
    return (paramid == null || paramid == '');
}

function Is_Display_Data() {
    // paramid has value
    return (!Is_ParamId_Blank());
}

function showMenu(p_user) {
    var menu = '';

    var style_hdr1 = 'font-size:14px !important; line-height: 17px; margin-left:10px !important;padding-left:5px !important; ';
    var style_menu = '';
    var style_menu2 = '';
    var icon = '';
    var url = '';

    var vc = 0;
    var activeItem = p_user.Menus.find(function (item) { return window.location.pathname === item.Url });
    
    p_user.Menus.forEach(
        function (item) {
            icon = 'ic-inventori';
            if (typeof (item.Logo) != 'undefined' && item.Logo != null) {
                icon = item.Logo;
            }
            if (item.hdr === 'hdr1') {
                vc += 1;
                style_menu = style_hdr1;
                style_menu2 = '';
                url = '#';

                if (vc > 1) {
                    menu += '</ul>'
                }

                menu += `<li data-toggle="collapse" data-target="#menu${vc.toString()}" class="active ${activeItem && activeItem.ParentId === item.MenuId ? '' : 'collapsed'}" aria-expanded="${activeItem && activeItem.ParentId === item.MenuId}">` +
                    //menu += '<li data-toggle="collapse" data-target="#menu' + vc.toString() + '" class="active collapsed" aria-expanded="false">' +
                    '<a class="nav-link" style="' + style_menu + '" href="' + url + '"><span style="' + style_menu2 + '"></span>' + item.MenuName + ' <span class="arrow"><i class="fa fa-angle-down" style="font-size:18px"></i></span></a>' +
                    '</li>';

                menu += `<ul class="nav-item collapse ${activeItem && activeItem.ParentId === item.MenuId ? 'show' : ''}" id="menu${vc.toString()}" style="list-style-type:none">`
                //menu += '<ul class="nav-item collapse" id="menu' + vc.toString() + '" style="list-style-type:none">'



            } else {
                var activeTab = 'border-radius: 5px; background-color: #fff;';
                var activeSpan = 'color: #463191 !important; font-weight: bold;';
                style_menu = '';
                style_menu2 = 'background: rgba(0, 0, 0, 0) url(\'/images/' + icon + '.png\') no-repeat scroll left top;';
                url = item.Url;

                menu += `<li class="nav-item" style="${window.location.pathname === url ? activeTab : ''}">` +
                    `<a class="nav-link ${window.location.pathname === url ? 'py-2' : ''}" style="${style_menu} ${window.location.pathname === url ? activeSpan : ''}" href="${url}">` +
                    '<span style="' + style_menu2 + '"></span>' + 
                    item.MenuName + 
                    '</a>' +
                    '</li>';
            }

        }
    );


    if (vc > 1) {
        menu += '</ul>'
    }

    //console.log(menu);
    menu = '<ul class="nav flex-column">' +
        menu +
        '    <li class="mercy_logout_line" style="margin-top:100px;padding:0px 10px !important;"></li>' +
        '    <li class="nav-item">' +
        '        <a class="nav-link" href="/?logout=1"><span></span><i class="fa fa-sign-out" style="font-size:18px" aria-hidden="true"></i>  Logout</a>' +
        '    </li>' +
        '</ul>'
        ;
    $('#mercy_menu').html(menu);
}

function Load_Page_List(){
    get_user_menu = '1';
    get_user_relation = '1';
    
    Populate_DataGrid();
}

function Load_Page_Form(){
    get_user_menu = '1';
    get_user_relation = '1';
    
    DisplayData();
}

function Display_Buttons(){
    // always reset
    $('#btnAdd').hide();
    
    try {
        
        // check based on Permission
        if (permission.Is_Add) $('#btnAdd').show();
        
    }catch(err){}
}


function Display_ButtonId(btnName){
    // always reset
    $('#'+btnName).hide();
    
    try {
        
        // check based on Permission
        if (permission.Is_Add) $('#' + btnName).show();
        
    }catch(err){}
}


function Display_Buttons_InLine_Editing(){
    try {
        if ( !permission.Is_Add && !permission.Is_Edit){
            obj_html_Table.columns('.mercy_icon_action').visible(false);
        }
    }catch(err){}
}

function Display_Buttons_Form(){
    // always reset
    $('#btnSave_outer').hide();
    $('#btnEdit_outer').hide();
    
    $('#btnCancel').html('Back');
    
    try {
        if (Mode_Create()){
            // check based on Permission
            if (permission.Is_Add){
                $('#btnSave_outer').show();
                
                $('#btnSave').html('Submit');
                $('#btnCancel').html('Cancel');
                
                return;
            }
        }
        
        if (Mode_View()){
            // check based on Permission
            if (permission.Is_Edit){
                $('#btnEdit_outer').show();
                
                $('#btnSave').html('Save');
                $('#btnCancel').html('Cancel');
                
                return;
            }
        }
        
        if (Mode_Edit()){
            // check based on Permission
            if (permission.Is_Edit){
                $('#btnSave_outer').show();
                
                $('#btnSave').html('Save');
                $('#btnCancel').html('Cancel');
                
                return;
            }
        }
    }catch(err){}
}

function Mode_Create(){
    return (paramid == null || paramid == '');
}

function Mode_View(){
    // paramid has value
    return (!Mode_Create() && !Mode_Edit());
}

var is_mode_edit = false;
function Mode_Edit(){
    return is_mode_edit;
}

function Feedback_Form_Show(){
    var mercy_Feedback_url = '/Feedbackv/Form';
    
    $("#mercy_Feedback_Inner").html('<iframe width="470px" height="610px" frameborder="0" scrolling="no" allowtransparency="true" src="'+mercy_Feedback_url+'?fromPage='+current_Page_URL+'"></iframe>');
    $("#mercy_Feedback").modal().appendTo("body");
}

function Feedback_Form_Close(){
    $("#mercy_Feedback").modal('hide');
}

var resize_Table_additional = 0;

function resize_Table() {
    if ($.cookie(MERCY_C_HIDE) == '1'){
        $("div.dataTables_wrapper").width('100%');
        return;
    }
    
    if ($('.mercy-sidebar').is(":hidden")){
        $("div.dataTables_wrapper").width('100%');
        return;
    }
    
    var width_outer = $('.mercy-box-outer').width();
    var width_sidebar = $('.mercy-sidebar').width();

    var width_additional = 20 + 20 + 40 + 10 + resize_Table_additional;
    var width_table = width_outer - width_sidebar - width_additional;
    if (width_table <= 0) {
        width_table = '100%';
    }
    
    $("div.dataTables_wrapper").width(width_table);
}

/*
* to get menu based on url
* @param {string} url
* @returns {string} menu if valid, otherwise empty string
*/
function getPathMenu(url) {
    try {
        const redirectUrl = new URL(url);
        // check if protocol is http or https
        if (redirectUrl.protocol !== 'http:' && redirectUrl.protocol !== 'https:') {
            return '';
        }
        // check if domain same with current domain
        if (redirectUrl.hostname !== window.location.hostname) {
            return '';
        }

        // check if path is exist
        const path = redirectUrl.pathname;
        if (path === '/' || !path) {
            return '';
        }

        return path;
    }
    catch (err) {
        return '';
    }
}

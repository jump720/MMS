var dtPageLength = 10;
var dtSearchDelay = 1000;
var ajaxModalIds = [];

$.MMS = {};

$(document).ready(function () {

    $.notifyDefaults({
        delay: 4000,
        animate: {
            enter: 'animated bounceIn',
            exit: 'animated bounceOutRight'
        },
        newest_on_top: true,
        offset: {
            x: 15,
            y: 85
        },
        mouse_over: "pause",
        z_index: 5000
    });

    fixMaterialCheckboxes();
    updateMaterialTextFields();

    $.fn.extend({
        animateCss: function (animationName, onEnd) {
            var animationEnd = 'webkitAnimationEnd mozAnimationEnd MSAnimationEnd oanimationend animationend';
            this.addClass('animated ' + animationName).one(animationEnd, function () {
                $(this).removeClass('animated ' + animationName);
                if (onEnd)
                    onEnd(this);
            });
        }
    });

    $("#userPictureProfile").click(function (event) {
        event.stopPropagation();
        $("#usernameLinkProfile").click();
    })
})

String.prototype.replaceAll = function (search, replacement) {
    var target = this;
    return target.replace(new RegExp(search, 'g'), replacement);
};

Date.prototype.toShortDate = function () {
    return (this.getDate() < 10 ? "0" : "") + this.getDate() +
        "/" + (this.getMonth() < 9 ? "0" : "") + (this.getMonth() + 1) +
        "/" + this.getFullYear();
}

Date.prototype.toShortDate2 = function () {
    return (this.getMonth() < 9 ? "0" : "") + (this.getMonth() + 1) +
        "/" + (this.getDate() < 10 ? "0" : "") + this.getDate() +
        "/" + this.getFullYear();
}

Date.prototype.toShortDateISO = function () {
    return this.getFullYear() +
        "-" + (this.getMonth() < 9 ? "0" : "") + (this.getMonth() + 1) +
        "-" + (this.getDate() < 10 ? "0" : "") + this.getDate();
}

Number.prototype.formatMoney = function (c, d, t) {
    var n = this,
        c = isNaN(c = Math.abs(c)) ? 2 : c,
        d = d == undefined ? "," : d,
        t = t == undefined ? "." : t,
        s = n < 0 ? "-" : "",
        i = String(parseInt(n = Math.abs(Number(n) || 0).toFixed(c))),
        j = (j = i.length) > 3 ? j % 3 : 0;
    return s + (j ? i.substr(0, j) + t : "") + i.substr(j).replace(/(\d{3})(?=\d)/g, "$1" + t) + (c ? d + Math.abs(n - i).toFixed(c).slice(2) : "");
};


function showChangePasswordModal(event, href) {
    ajaxModal(event, href, 3, {
        static: true,
        fnShown: function () {
            $.MMS.ChangePassword.init();
        },
        fnHidden: function () {
            // dt.api().draw('page');
        }
    });
}


function refreshSecurity() {
    sLoading();
    $.ajax({
        url: '/usuarios/actualizaSeguridad',
        contentType: 'application/html; charset=utf-8',
        type: 'GET',
        dataType: 'json'
    })
        .success(function (result) {
            if (result)
                msgSuccess("Security refreshed correctly. Click <a href='javascript:void(0)' onclick='location.reload()' class='alert-link'>here</a> to reload this page.");
            else
                msgError("User's session has expired.");

            hLoading();
        })
        .error(function (xhr, status) {
            Materialize.toast(status, 4000)
            $('#Loading').closeModal();
        });
}

function sLoading(text) {
    text = text || "Loading...";
    $.LoadingOverlay("show", {
        color: "rgba(0, 0, 0, 0.3)",
        image: "",
        custom: `<h3 class="col-white">${text}</h3><div class="preloader pl-size-xl"><div class="spinner-layer pl-white"><div class="circle-clipper left"><div class="circle"></div></div><div class="circle-clipper right"><div class="circle"></div></div></div></div>`,
        fade: false
    });
}

function hLoading() {
    $.LoadingOverlay("hide");
}

function getNotifySettings(type) {
    return {
        type: type,
        delay: 4000,
        animate: {
            enter: 'animated bounceIn',
            exit: 'animated bounceOutRight'
        },
        newest_on_top: true,
        offset: {
            x: 15,
            y: 85
        }
    };
}

function msgSuccess(message, title) {
    title = title || "";
    $.notify({
        title: '<strong>' + title + '</strong>',
        message: message
    }, {
            type: "success"
        });
}

function msgInfo(message, title) {
    title = title || "";
    $.notify({
        title: '<strong>' + title + '</strong>',
        message: message
    }, {
            type: "info"
        });
}

function msgAlert(message, title) {
    title = title || "";
    $.notify({
        title: '<strong>' + title + '</strong>',
        message: message
    }, {
            type: "warning"
        });
}

function msgError(message, title) {
    title = title || "";
    $.notify({
        title: '<strong>' + title + '</strong>',
        message: message
    }, {
            type: "danger"
        });
}

function getUrlParam(name, url) {
    if (!url) url = location.href;
    name = name.replace(/[\[\]]/g, "\\$&");
    var regex = new RegExp("[?&]" + name + "(=([^&#]*)|&|#|$)"),
        results = regex.exec(url);
    if (!results) return null;
    if (!results[2]) return '';
    return decodeURIComponent(results[2].replace(/\+/g, " "));
}

function isImageFile(file) {
    if (file.type) {
        return /^image\/\w+$/.test(file.type);
    } else {
        return /\.(jpg|jpeg|png)$/.test(file);
    }
}

function randomNumber() {
    return (new Date().getTime() * Math.random()).toFixed(0);
}

function htmlCheckbox(value) {
    return value ? '<input type="checkbox" checked disabled /><label class="m-b--10"></label>' : "";
}

function dataTablesIndex(tableId, urlSource, urlPush, columns, options) {
    var needPushState = true;
    var searchValues = getSearchValues();

    options = options || {};

    var opts = {
        mf: options.mainFilter == undefined ? true : options.mainFilter,
        fc: options.filterControls || [],
        spcb: options.fnServerParams || function () { return []; },
        sfc: options.fnSetFilterControls || function () { },
        md: options.modalTable == undefined ? false : options.modalTable,
    };

    setSearchControls(searchValues, opts.fc, opts.sfc);

    var dom = `<'row'<'col-sm-6'l>"${(opts.mf ? "<'col-sm-6'f>" : "")}">` +
        "<'row'<'col-sm-12'tr>>" +
        "<'row'<'col-sm-5'i><'col-sm-7'p>>";

    dt = $("#" + tableId).dataTable({
        dom: dom,
        //responsive: true,
        pageLength: getUrlParam("dtDL") || dtPageLength,
        displayStart: getUrlParam("dtDS") || 0,
        searchDelay: dtSearchDelay,
        search: { search: searchValues[0] || "" },
        sort: false,
        processing: true,
        serverSide: true,
        ajax: {
            url: urlSource,
            type: "post"
        },
        columns: columns,
        fnServerParams: function (data) {
            var serverParams = opts.spcb();

            for (var i in serverParams)
                data[serverParams[i].name] = serverParams[i].value;
        }
    });

    dt.fnFilterOnReturn();

    for (var i in opts.fc) {
        var fcId = "#" + opts.fc[i];

        if ($(fcId).is("input:text")) {
            $(fcId).keypress(function (e) {
                if (e.which == 13)
                    dt.api().draw();
            });
            $(fcId).blur(function (e) {
                if ($(this).attr('data-oldvalue') != $(this).val())
                    dt.api().draw();
            });
            $(fcId).focus(function (e) {
                $(this).attr('data-oldvalue', $(this).val());
            });
        }
        else if ($(fcId).is("select")) {
            $(fcId).change(function (e) {
                dt.api().draw();
            });
        }
    }

    dt.on('draw.dt', function () {
        $(`#${tableId} [data-toggle="tooltip"]`).tooltip();

        var dtPageInfo = dt.api().page.info();
        var dtS = dt.api().search().replaceAll(",", ";;");

        for (var i in opts.fc)
            dtS += "," + ($("#" + opts.fc[i]).val() || "").replaceAll(",", ";;");

        var dtActualDS = getUrlParam("dtDS") || 0;
        var dtActualDL = getUrlParam("dtDL") || dtPageLength;
        var dtActualS = getUrlParam("dtS") || ",".repeat(opts.fc.length);

        if (!opts.md)
            if (dt.api().ajax.params().draw > 1 && needPushState && (dtActualDS != dtPageInfo.start || dtActualDL != dtPageInfo.length || dtActualS != dtS))
                window.history.pushState("", "", urlPush + (urlPush.includes("?") ? "&" : "?") + "dtDS=" + dtPageInfo.start + "&dtDL=" + dtPageInfo.length + "&dtS=" + encodeURIComponent(dtS));

        needPushState = true;
    });

    $(window).bind('popstate', function (e) {
        opts.fc = getSearchValues();
        dt.api().search(opts.fc[0] || "");
        setSearchControls(searchValues, opts.fc, opts.sfc);
        dt.api().page.len(getUrlParam("dtDL") || dtPageLength).page((getUrlParam("dtDS") / getUrlParam("dtDL")) || 0).draw('page');
        needPushState = false;
    });

    return dt;
}

function getSearchValues() {
    var searchValues = getUrlParam("dtS") == null ? [] : decodeURIComponent(getUrlParam("dtS")).split(",");

    for (var i in searchValues)
        searchValues[i] = searchValues[i].replaceAll(";;", ",");

    return searchValues;
}

function setSearchControls(searchValues, searchControls, callback) {
    for (var i in searchControls) {
        var j = parseInt(i) + 1;
        $("#" + searchControls[i]).val(searchValues[j]);

        if ($("#" + searchControls[i]).data("select2"))
            $("#" + searchControls[i]).change();
    }

    if (typeof callback === "function")
        callback(searchValues);
}

function redirtWithReturnSearch(item, event) {
    if (event)
        event.preventDefault();

    if (location.search == "")
        location.href = item.href;
    else
        location.href = item.href + (item.href.includes("?") ? "&" : "?") + "returnSearch=" + encodeURIComponent(location.search);

    return false;
}

function redirToReturnSearch(item, event) {
    if (event)
        event.preventDefault();

    var returnSearch = getUrlParam("returnSearch");
    var urlSearch = "";

    if (returnSearch != null && returnSearch.trim() != "")
        urlSearch = decodeURIComponent(returnSearch);

    if (item.href.includes("?"))
        urlSearch.replace("?", "&");

    location.href = item.href + urlSearch;
    return false;
}

function getUrlWithReturnSearch(url) {
    var returnSearch = getUrlParam("returnSearch");
    var urlSearch = "";

    if (returnSearch != null && returnSearch.trim() != "")
        urlSearch = (url.includes("?") ? "&" : "?") + "returnSearch=" + encodeURIComponent(returnSearch);

    return url + urlSearch;
}

function fixMaterialCheckboxes(container) {
    if (container)
        container += " ";
    else
        container = "";

    $(container + ".fix-checkbox").each(function (index, item) {
        $("#" + $(item).attr("for")).after($(item));
        $(item).removeClass("fix-checkbox");
    });
}

function updateMaterialTextFields(container) {
    container = container ? container + " " : "";

    var input_selector = `${container}input[type=text], ${container}input[type=password], ${container}input[type=email], ${container}input[type=url], ${container}input[type=tel], ${container}input[type=number], ${container}input[type=search], ${container}input[type=datetime], ${container}textarea`;
    $(input_selector).each(function (index, element) {
        if ($(element).val().length > 0 || element.autofocus || $(this).attr('placeholder') !== undefined || $(element)[0].validity.badInput === true) {
            $(this).siblings('label').addClass('active');
        }
        else {
            $(this).siblings('label').removeClass('active');
        }
    });
    $.AdminBSB.input.activate(container, false);
}

function checkValidationSummaryErrors(containerId) {
    containerId = containerId || "cardForm";

    var validationSummaryErrors = $("#" + containerId + " .validation-summary-errors").html();

    if (validationSummaryErrors)
        msgError(validationSummaryErrors, "Error");
}

function ajaxModal(event, url, size, options) {
    event.preventDefault();
    url = url || "";

    if (!url)
        return;

    size = size || 2
    options = options || {};

    var data = options.data,
        staticBackdrop = options.static || false,
        forced = options.forced || false,
        onHideCallback = options.fnHide || function () { },
        onHiddenCallback = options.fnHidden || function () { },
        onShowCallback = options.fnShow || function () { },
        onShownCallback = options.fnShown || function () { },
        modalSize = "";

    if (size == 1)
        modalSize = "modal-sm";
    else if (size == 3)
        modalSize = "modal-lg";
    else if (size == 4)
        modalSize = "modal-xl";

    sLoading();

    var modalId = guid();
    var done = function (result) {
        $(`<div id="${modalId}" class="modal" role="dialog" ${staticBackdrop ? 'data-keyboard="false" data-backdrop="static"' : ""}><div class="modal-dialog ${modalSize}" role="document"><div class="modal-content">${result}</div></div></div>`)
            .on('show.bs.modal', function () {
                onShowCallback();
            })
            .on('shown.bs.modal', function () {
                fixMaterialCheckboxes("#" + modalId);
                $.AdminBSB.input.activate("#" + modalId);
                $.AdminBSB.select.activate("#" + modalId);
                updateMaterialTextFields("#" + modalId);
                onShownCallback();
            })
            .on('hide.bs.modal', function () {
                onHideCallback();
            })
            .on('hidden.bs.modal', function () {
                $(this).remove();
                ajaxModalIds.splice(ajaxModalIds.indexOf(this.id), 1);
                onHiddenCallback();
            })
            .modal();

        if (forced)
            $('#' + modalId + " .modal-header .close").remove();

        ajaxModalIds.push(modalId);
    };

    if (data) { // POST
        $.post(url, data, function (result) {
            done(result);
        }).fail(function (jqXHR, textStatus, errorThrown) {
            msgError(errorThrown);
        }).always(function () {
            hLoading();
        });
    }
    else { // GET
        $.get(url, function (result) {
            done(result);
        }).fail(function (jqXHR, textStatus, errorThrown) {
            msgError(errorThrown);
        }).always(function () {
            hLoading();
        });
    }
}

function getCurrentModalId() {
    return ajaxModalIds[ajaxModalIds.length - 1];
}

function guid() {
    var s4 = function () {
        return Math.floor((1 + Math.random()) * 0x10000)
            .toString(16)
            .substring(1);
    }

    return s4() + s4() + '-' + s4() + '-' + s4() + '-' +
        s4() + '-' + s4() + s4() + s4();
}

function sModal(modalId) {
    $("#" + modalId).modal();
}

function hModal(modalId) {
    $("#" + modalId).modal("hide");
}

function replaceCommas() {
    for (var i = 0; i < arguments.length; i++)
        $(arguments[i]).val($(arguments[i]).val().replaceAll(",", "."));
}

function handleError(jqXHR, textStatus, errorThrown) {
    if (jqXHR.responseJSON && jqXHR.responseJSON.Message)
        msgError(jqXHR.responseJSON.Message);
    else
        msgError(errorThrown);
}


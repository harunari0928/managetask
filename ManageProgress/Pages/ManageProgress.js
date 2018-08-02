﻿"use strict";
let param, json, participants, tasks;
window.onload = () => {
    let urlParam = location.search.substring(1);
    if (urlParam) {
        param = urlParam.split('=');
    }
    AjaxCommunication();

};
function AjaxCommunication() {
    let data = JSON.stringify({ strProgressId: param[1] });
    $.ajax({
        type: "POST",
        url: "../API/GetJsonString.aspx/GetParticipants",
        data: data,
        async: true,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        cache: false,
        timeout: 5000,
        success: OnSuccessGetParticipants,
        error: (xhr, ajaxOptions, thrownError) => {
            alert("通信に失敗しました");
        }
    });
    $.ajax({
        type: "POST",
        url: "../API/GetJsonString.aspx/GetTasks",
        data: data,
        async: true,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        cache: false,
        timeout: 5000,
        success: OnSuccessGetTasks,
        error: (xhr, ajaxOptions, thrownError) => {
            alert("通信に失敗しました");
        }
    });
}
function CreateTable(members, tks) {
    let tag;
    let width = (members.length + 2) * 50;
    console.log(width);
    tag = "" +
        "<div id='taskTable' style='overflow:auto;width:" + width + "px'>" +
        "<div class='d-table-row'>" +
        "<p class='d-table-cell p-2 bg-dark text-white'>" +
        "タスク名" +
        "</p>";
    for (let i = 0; i < members.length; i++) {
        tag += "<p class='d-table-cell p-2 bg-dark text-white'>" +
            members[i].participantName +
            "</p>";
    }
    tag += "</div>";
    for (let i = 1; i <= tks.length; i++) {
        tag += "" +
            "<div class='d-table-row'>" +
            "<p class='d-table-cell p-2 bg-dark text-white'>" +
            i + "." +
            tks[i - 1].task +
            "</p>";
        for (let j = 0; j < members.length; j++) {
            // tks[i].taskのタスクが終わっているかどうか
            if (members[j].currentProgress >= i) {
                tag += "<p class='d-table-cell p-2 bg-primary text-white'>済み</p>";
            } else {
                tag += "<p class='d-table-cell p-2  text-white' style='background-color: gray;'></p>";
            }
        }
        tag += "</div>";
    }
    tag += "</div>";
    let resultTag = document.getElementById("tb");
    resultTag.innerHTML = tag;
}
function OnSuccessGetParticipants(response) {
    if (response.d === "error") {
        alert("サーバでエラーが発生しました");
    }
    participants = JSON.parse(response.d);
}
function OnSuccessGetTasks(response) {
    if (response.d === "error") {
        alert("サーバでエラーが発生しました");
    }
    tasks = JSON.parse(response.d);
    CreateTable(participants, tasks);
    $('#join').modal('hide');
}
function addParticipantDataToDb() {
    // バリデーション
    let password = $('#password').val();
    let name = $('#name').val();
    if (name === "") {
        $('#nameAlert').css('color', 'red');
        $('#nameAlert').text('名前が入力されていません');

    } else {
        $('#nameAlert').text('');
    }
    if (password === "") {
        $('#passAlert').css('color', 'red');
        $('#passAlert').text('パスワードが入力されていません');
    } else {
        $('#passAlert').text('');
    }
    if (name === "" || password === "") {
        return;
    }

    json = {
        progressId: param[1],
        name: name,
        password: password
    };

    console.log(json);
    let data = JSON.stringify({ jsonString: JSON.stringify(json) });

    $.ajax({
        type: "POST",
        url: "../API/SetJsonString.aspx/SetNewParticipant",
        data: data,
        async: true,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        cache: false,
        success: OnSuccessRegister,
        error: (xhr, ajaxOptions, thrownError) => {
            alert("通信に失敗しました");
        }
    });
}
function OnSuccessRegister(responce) {
    console.log("aaa");
    if (responce.d === "wrongPassword") {
        $('#passAlert').css('color', 'red');
        $('#passAlert').text('パスワードが異なります');
    }
    if (responce.d === "sameNameExisted") {
        $('#nameAlert').css('color', 'red');
        $('#nameAlert').text('同じ名前が既に登録されています');
    }
    if (responce.d === "registeredSuccess") {
        $('#passAlert').text('');
        $('#nameAlert').text('');
        AjaxCommunication();
    }
    if (responce.d === "error") {
        alert("エラーが発生しました");
    }
}
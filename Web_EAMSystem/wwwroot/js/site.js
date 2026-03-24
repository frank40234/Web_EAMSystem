// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
// wwwroot/js/site.js

$(document).ready(function () {
    // 🌟 全域魔法：只要網頁上的下拉選單帶有 'select2-enable' 這個 class，就自動把它變成可搜尋的選單！
    $('.select2-enable').select2({
        width: '100%',
        language: {
            noResults: function () {
                return "找不到符合的結果";
            }
        }
    });
});
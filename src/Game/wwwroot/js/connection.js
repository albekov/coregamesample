/// <reference path="../../typings/_references.ts" />
var Connection = (function () {
    function Connection() {
        this.authHub = $.connection.authHub;
        this.gameHub = $.connection.gameHub;
        $.connection.hub.logging = true;
        this.authHub.client.message = function (data) {
            console.log(">> message: " + data);
        };
        this.authHub.client.status = function (data) {
            $('#status').text(data);
        };
    }
    Connection.prototype.connect = function () {
        var _this = this;
        $.connection.hub.start()
            .done(function () {
            _this.init();
        });
    };
    Connection.prototype.init = function () {
        var _this = this;
        this.onLogging();
        this.authHub.server
            .init(localStorage['authId'])
            .then(function (authId) { return _this.handleLogin(authId); });
    };
    Connection.prototype.login = function (username, password) {
        var _this = this;
        this.onLogging();
        this.authHub.server
            .login(username, password)
            .then(function (authId) { return _this.handleLogin(authId); });
    };
    Connection.prototype.handleLogin = function (authId) {
        if (authId) {
            localStorage['authId'] = authId;
            this.onLoggedIn();
        }
        else {
            this.onLoggedOut();
        }
    };
    Connection.prototype.logout = function () {
        var _this = this;
        this.authHub.server.logout()
            .then(function () {
            localStorage['authId'] = null;
            _this.init();
        });
    };
    Connection.prototype.game = function () {
        return this.gameHub.server;
    };
    return Connection;
}());

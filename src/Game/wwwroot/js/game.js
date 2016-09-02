/// <reference path="../../typings/_references.ts" />
var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var StartState = (function (_super) {
    __extends(StartState, _super);
    function StartState() {
        _super.apply(this, arguments);
    }
    StartState.prototype.create = function () {
        console.log('StartState.create');
    };
    StartState.prototype.init = function () {
        console.log('StartState.init');
    };
    StartState.prototype.preload = function () {
        console.log('StartState.preload');
    };
    return StartState;
}(Phaser.State));
var MainState = (function (_super) {
    __extends(MainState, _super);
    function MainState() {
        _super.apply(this, arguments);
    }
    MainState.prototype.create = function () {
        console.log('MainState.create');
    };
    MainState.prototype.init = function () {
        console.log('MainState.init');
    };
    MainState.prototype.preload = function () {
        console.log('MainState.preload');
    };
    return MainState;
}(Phaser.State));
var GameState = (function () {
    function GameState() {
    }
    GameState.start = 'start';
    GameState.main = 'main';
    return GameState;
}());
var Game = (function () {
    function Game(connection) {
        var _this = this;
        //#region Login/Logout
        this.loginButton = $('#login-submit');
        this.guestButton = $('#login-guest');
        this.logoutButton = $('#logout');
        this.connection = connection;
        this.connection.onLoggedIn = function () { return _this.loggedIn(); };
        this.connection.onLoggedOut = function () { return _this.loggedOut(); };
        this.connection.onLogging = function () { return _this.logging(); };
        this.initLoginEvents();
    }
    Game.prototype.start = function () {
        var _this = this;
        this.game = new Phaser.Game(600, 400, Phaser.AUTO, 'game', {
            preload: function () { return _this.preload(); },
            create: function () { return _this.create(); }
        });
    };
    Game.prototype.stop = function () {
        if (this.game) {
            this.game.destroy();
            this.game = null;
        }
    };
    Game.prototype.initStates = function () {
        this.game.state.add(GameState.start, new StartState());
        this.game.state.add(GameState.main, new MainState());
        this.game.state.start(GameState.start);
    };
    Game.prototype.preload = function () {
    };
    Game.prototype.create = function () {
        this.game.physics.startSystem(Phaser.Physics.ARCADE);
        this.game.world.setBounds(-1000, -1000, 2000, 2000);
        this.initStates();
        this.connection.game().start();
    };
    Game.prototype.initLoginEvents = function () {
        var _this = this;
        this.loginButton.on('click', function () { return _this.doLogin(); });
        this.guestButton.on('click', function () { return _this.doLoginGuest(); });
        this.logoutButton.on('click', function () { return _this.doLogout(); });
    };
    Game.prototype.doLogin = function () {
        var username = $('#login-username').val();
        var password = $('#login-password').val();
        this.connection.login(username, password);
    };
    Game.prototype.doLoginGuest = function () {
        this.connection.login('guest', 'guest');
    };
    Game.prototype.doLogout = function () {
        this.connection.logout();
    };
    Game.prototype.loggedIn = function () {
        console.log('loggedIn');
        this.showPage('game');
        this.start();
    };
    ;
    Game.prototype.loggedOut = function () {
        console.log('loggedOut');
        this.stop();
        $('#login-form').trigger('reset');
        this.showPage('login');
    };
    ;
    Game.prototype.logging = function () {
        console.log('logging');
        this.showPage('loading');
    };
    ;
    Game.prototype.showPage = function (page) {
        $('#wrapper > div').hide();
        $("#" + page + "-container").show();
    };
    return Game;
}());

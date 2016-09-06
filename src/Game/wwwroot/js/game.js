/// <reference path="../../typings/_references.ts" />
var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
function log() {
    var args = [];
    for (var _i = 0; _i < arguments.length; _i++) {
        args[_i - 0] = arguments[_i];
    }
    if (debug)
        console.log.apply(console, args);
}
var StartState = (function (_super) {
    __extends(StartState, _super);
    function StartState() {
        _super.call(this);
        log('StartState.ctor()');
    }
    StartState.prototype.create = function () {
        log('StartState.create');
        this.game.world.setBounds(0, 0, this.game.width, this.game.height);
        var startText = this.game.add.text(this.game.world.centerX, this.game.world.centerY, 'START', {
            font: "65px 'Gill Sans', 'Gill Sans MT', Calibri, 'Trebuchet MS', sans-serif",
            fill: '#4400FF',
            align: 'center'
        });
        startText.anchor.set(0.5);
        startText.inputEnabled = true;
        startText.events.onInputUp.add(this.start, this);
        var logoutText = this.game.add.text(10, 5, 'LOGOUT', {
            font: "18px 'Gill Sans', 'Gill Sans MT', Calibri, 'Trebuchet MS', sans-serif",
            fill: '#FFFFFF'
        });
        logoutText.inputEnabled = true;
        logoutText.events.onInputUp.add(this.logout, this);
    };
    StartState.prototype.init = function (connection) {
        log('StartState.init');
        this.connection = connection;
    };
    StartState.prototype.preload = function () {
        log('StartState.preload');
    };
    StartState.prototype.start = function () {
        this.connection.startGame();
    };
    StartState.prototype.logout = function () {
        this.connection.logout();
    };
    return StartState;
}(Phaser.State));
var MainState = (function (_super) {
    __extends(MainState, _super);
    function MainState() {
        _super.call(this);
        log('MainState.ctor()');
    }
    MainState.prototype.create = function () {
        log('MainState.create');
        this.game.add.tileSprite(this.world.bounds.x, this.world.bounds.y, this.world.bounds.width, this.world.bounds.height, 'background');
        var stopText = this.game.add.text(10, 5, 'EXIT', {
            font: "18px 'Gill Sans', 'Gill Sans MT', Calibri, 'Trebuchet MS', sans-serif",
            fill: '#FFFFFF'
        });
        stopText.inputEnabled = true;
        stopText.events.onInputUp.add(this.onExit, this);
        stopText.fixedToCamera = true;
        this.game.input.onDown.add(this.clicked, this);
    };
    MainState.prototype.init = function (connection, data) {
        log('MainState.init');
        this.connection = connection;
        this.data = data;
        this.playerId = data.player.id;
        log('playerId', this.playerId);
        this.entities = [];
        var wi = data.world;
        this.world.setBounds(wi.x0, wi.y0, wi.width, wi.height);
    };
    MainState.prototype.clicked = function () {
        log('clicked');
        var x = this.game.input.activePointer.worldX;
        var y = this.game.input.activePointer.worldY;
        log(x, y);
        this.connection.moveTo(x, y);
    };
    MainState.prototype.preload = function () {
        log('MainState.preload');
        this.game.load.image('background', '/images/deep-space.jpg');
    };
    MainState.prototype.update = function () {
        if (Game.data) {
            var data = Game.data;
            Game.data = null;
            if (data.entities && data.entities.updated) {
                var entities = data.entities.updated;
                for (var _i = 0, entities_1 = entities; _i < entities_1.length; _i++) {
                    var re = entities_1[_i];
                    if (!this.hasEntity(re.id))
                        this.addEntity(re);
                    this.updateEntity(re);
                }
            }
        }
    };
    MainState.prototype.render = function () {
        if (debug) {
            this.game.debug.inputInfo(32, 40);
            this.game.debug.cameraInfo(this.game.camera, 300, 40);
        }
    };
    MainState.prototype.onExit = function () {
        this.connection.stopGame();
    };
    MainState.prototype.getEntityById = function (id) {
        return this.entities[id];
    };
    MainState.prototype.addEntity = function (re) {
        var g = this.createEntity(re);
        this.physics.enable(g, Phaser.Physics.ARCADE);
        g.body.velocity.x = re.dx;
        g.body.velocity.y = re.dy;
        if (re.type === 'player' && re.id === this.playerId) {
            this.camera.follow(g, Phaser.Camera.FOLLOW_LOCKON, 0.1, 0.1);
            this.camera.deadzone = new Phaser.Rectangle(150, 150, this.camera.width - 300, this.camera.height - 300);
        }
        re.obj = g;
        this.entities[re.id] = re;
    };
    MainState.prototype.updateEntity = function (re) {
        var e = this.getEntityById(re.id);
        var g = e.obj;
        g.x = re.x;
        g.y = re.y;
        g.body.velocity.x = re.dx;
        g.body.velocity.y = re.dy;
    };
    MainState.prototype.hasEntity = function (id) {
        return id in this.entities;
    };
    MainState.prototype.createEntity = function (re) {
        var g = this.add.graphics(re.x, re.y);
        switch (re.type) {
            case 'player':
                g.lineStyle(4, 0xFF0000);
                g.drawRect(-7, -7, 14, 14);
                break;
            case 'food':
                g.lineStyle(2, 0x00FF00);
                g.drawRect(-5, -5, 10, 10);
        }
        return g;
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
        this.connection.onStart = function (data) { return _this.started(data); };
        this.connection.onStop = function () { return _this.stopped(); };
        this.connection.onUpdate = function (data) { return _this.gameUpdate(data); };
        this.initLoginEvents();
    }
    Game.prototype.start = function () {
        var _this = this;
        this.game = new Phaser.Game(600, 400, Phaser.AUTO, 'game', {
            preload: function () { return _this.preload(); },
            create: function () { return _this.create(); },
            render: function () { return _this.render(); }
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
        this.game.state.start(GameState.start, true, false, this.connection);
    };
    Game.prototype.started = function (data) {
        this.game.state.start(GameState.main, true, false, this.connection, data);
    };
    Game.prototype.stopped = function () {
        this.game.state.start(GameState.start, true, false, this.connection);
    };
    Game.prototype.gameUpdate = function (data) {
        Game.data = data;
    };
    Game.prototype.preload = function () {
    };
    Game.prototype.create = function () {
        this.game.physics.startSystem(Phaser.Physics.ARCADE);
        //this.game.add.plugin(Phaser.Plugin.Debug);
        this.game.stage.disableVisibilityChange = true;
        this.initStates();
    };
    Game.prototype.render = function () {
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
        log('loggedIn');
        this.showPage('game');
        this.start();
    };
    ;
    Game.prototype.loggedOut = function () {
        log('loggedOut');
        this.stop();
        $('#login-form').trigger('reset');
        this.showPage('login');
    };
    ;
    Game.prototype.logging = function () {
        log('logging');
        this.showPage('loading');
    };
    ;
    Game.prototype.showPage = function (page) {
        $('#wrapper > div').hide();
        $("#" + page + "-container").show();
    };
    return Game;
}());

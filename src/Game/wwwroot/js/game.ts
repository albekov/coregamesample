/// <reference path="../../typings/_references.ts" />

declare var debug: boolean;

function log(...args) {
    if (debug)
        console.log.apply(console, args);
}

class StartState extends Phaser.State {
    private connection: IConnection;

    constructor() {
        super();
        log('StartState.ctor()');
    }

    create() {
        log('StartState.create');
        this.game.world.setBounds(0, 0, this.game.width, this.game.height);
        const startText = this.game.add.text(
            this.game.world.centerX,
            this.game.world.centerY,
            'START',
            {
                font: "65px 'Gill Sans', 'Gill Sans MT', Calibri, 'Trebuchet MS', sans-serif",
                fill: '#4400FF',
                align: 'center'
            });
        startText.anchor.set(0.5);
        startText.inputEnabled = true;
        startText.events.onInputUp.add(this.start, this);

        const logoutText = this.game.add.text(
            10, 5,
            'LOGOUT',
            {
                font: "18px 'Gill Sans', 'Gill Sans MT', Calibri, 'Trebuchet MS', sans-serif",
                fill: '#FFFFFF'
            });
        logoutText.inputEnabled = true;
        logoutText.events.onInputUp.add(this.logout, this);
    }

    init(connection: IConnection) {
        log('StartState.init');
        this.connection = connection;
    }

    preload() {
        log('StartState.preload');
    }

    start() {
        this.connection.startGame();
    }

    logout() {
        this.connection.logout();
    }
}

class MainState extends Phaser.State {
    private connection: IConnection;
    private data: any;
    private playerId: string;

    constructor() {
        super();
        log('MainState.ctor()');
    }

    create() {
        log('MainState.create');

        this.game.add.tileSprite(this.world.bounds.x, this.world.bounds.y, this.world.bounds.width, this.world.bounds.height, 'background');

        const stopText = this.game.add.text(
            10, 5,
            'EXIT',
            {
                font: "18px 'Gill Sans', 'Gill Sans MT', Calibri, 'Trebuchet MS', sans-serif",
                fill: '#FFFFFF'
            });
        stopText.inputEnabled = true;
        stopText.events.onInputUp.add(this.onExit, this);
        stopText.fixedToCamera = true;

        this.game.input.onDown.add(this.clicked, this);
    }

    init(connection: IConnection, data: any) {
        log('MainState.init');
        this.connection = connection;
        this.data = data;
        this.playerId = data.player.id;
        log('playerId', this.playerId);
        this.entities = [];

        const wi = data.world;
        this.world.setBounds(wi.x0, wi.y0, wi.width, wi.height);
    }

    clicked() {
        log('clicked');
        var x = this.game.input.activePointer.worldX;
        var y = this.game.input.activePointer.worldY;
        log(x, y);
        this.connection.moveTo(x, y);
    }

    preload() {
        log('MainState.preload');
        this.game.load.image('background', '/images/deep-space.jpg');
    }

    private entities: any;

    update() {
        if (Game.data) {
            const data = Game.data;
            Game.data = null;

            if (data.entities && data.entities.updated) {
                const entities = data.entities.updated;

                for (const re of entities) {
                    if (!this.hasEntity(re.id))
                        this.addEntity(re);

                    this.updateEntity(re);
                }
            }
        }
    }

    render() {
        if (debug) {
            this.game.debug.inputInfo(32, 40);
            this.game.debug.cameraInfo(this.game.camera, 300, 40);
        }
    }

    onExit() {
        this.connection.stopGame();
    }

    getEntityById(id: string): IGameEntity {
        return this.entities[id];
    }

    addEntity(re: IGameEntity) {
        const g = this.createEntity(re);
        this.physics.enable(g, Phaser.Physics.ARCADE);
        g.body.velocity.x = re.dx;
        g.body.velocity.y = re.dy;

        if (re.type === 'player' && re.id === this.playerId) {
            this.camera.follow(g, Phaser.Camera.FOLLOW_LOCKON, 0.1, 0.1);
            this.camera.deadzone = new Phaser.Rectangle(150, 150, this.camera.width - 300, this.camera.height - 300);
        }

        re.obj = g;
        this.entities[re.id] = re;
    }

    updateEntity(re: IGameEntity) {
        const e = this.getEntityById(re.id);
        const g = e.obj;
        g.x = re.x;
        g.y = re.y;
        g.body.velocity.x = re.dx;
        g.body.velocity.y = re.dy;
    }

    hasEntity(id: string) {
        return id in this.entities;
    }

    createEntity(re) {
        const g = this.add.graphics(re.x, re.y);
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
    }
}

class GameState {
    static start = 'start';
    static main = 'main';
}

interface IGameUpdate {
    entities: IEntitiesUpdate;
}

interface IEntitiesUpdate {
    updated: IGameEntity[];
}

interface IGameEntity {
    id: string;
    type: string;
    name: string;
    x: number;
    y: number;
    dx: number;
    dy: number;
    obj: Phaser.Graphics;
}

class Game {
    private connection: IConnection;
    private game: Phaser.Game;
    static data: IGameUpdate;

    constructor(connection: IConnection) {
        this.connection = connection;
        this.connection.onLoggedIn = () => this.loggedIn();
        this.connection.onLoggedOut = () => this.loggedOut();
        this.connection.onLogging = () => this.logging();

        this.connection.onStart = (data: any) => this.started(data);
        this.connection.onStop = () => this.stopped();

        this.connection.onUpdate = data => this.gameUpdate(data);

        this.initLoginEvents();
    }

    private start() {
        this.game = new Phaser.Game(600,
            400,
            Phaser.AUTO,
            'game',
            {
                preload: () => this.preload(),
                create: () => this.create(),
                render: () => this.render()
            });
    }

    private stop() {
        if (this.game) {
            this.game.destroy();
            this.game = null;
        }
    }

    private initStates() {
        this.game.state.add(GameState.start, new StartState());
        this.game.state.add(GameState.main, new MainState());
        this.game.state.start(GameState.start, true, false, this.connection);
    }

    started(data) {
        this.game.state.start(GameState.main, true, false, this.connection, data);
    }

    stopped() {
        this.game.state.start(GameState.start, true, false, this.connection);
    }

    gameUpdate(data: IGameUpdate) {
        Game.data = data;
    }

    preload() {
    }

    create() {
        this.game.physics.startSystem(Phaser.Physics.ARCADE);
        //this.game.add.plugin(Phaser.Plugin.Debug);
        this.game.stage.disableVisibilityChange = true;
        this.initStates();
    }

    render() {
    }

    //#region Login/Logout

    private loginButton = $('#login-submit');
    private guestButton = $('#login-guest');
    private logoutButton = $('#logout');

    initLoginEvents() {
        this.loginButton.on('click', () => this.doLogin());
        this.guestButton.on('click', () => this.doLoginGuest());
        this.logoutButton.on('click', () => this.doLogout());
    }

    doLogin() {
        const username = $('#login-username').val();
        const password = $('#login-password').val();
        this.connection.login(username, password);
    }

    doLoginGuest() {
        this.connection.login('guest', 'guest');
    }

    doLogout() {
        this.connection.logout();
    }

    loggedIn() {
        log('loggedIn');
        this.showPage('game');
        this.start();
    };

    loggedOut() {
        log('loggedOut');
        this.stop();
        $('#login-form').trigger('reset');
        this.showPage('login');
    };

    logging() {
        log('logging');
        this.showPage('loading');
    };

    showPage(page: string) {
        $('#wrapper > div').hide();
        $(`#${page}-container`).show();
    }

    //#endregion
}
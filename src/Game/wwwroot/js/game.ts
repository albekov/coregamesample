/// <reference path="../../typings/_references.ts" />

class StartState extends Phaser.State {
    private connection: IConnection;

    constructor() {
        super();
        console.log('StartState.ctor()');
    }

    create() {
        console.log('StartState.create');
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
        console.log('StartState.init');
        this.connection = connection;
    }

    preload() {
        console.log('StartState.preload');
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

    constructor() {
        super();
        console.log('MainState.ctor()');
    }

    create() {
        console.log('MainState.create');
        const stopText = this.game.add.text(
            10, 5,
            'EXIT',
            {
                font: "18px 'Gill Sans', 'Gill Sans MT', Calibri, 'Trebuchet MS', sans-serif",
                fill: '#FFFFFF'
            });
        //stopText.anchor.set(0.5);
        stopText.inputEnabled = true;
        stopText.events.onInputUp.add(this.onExit, this);
        stopText.fixedToCamera = true;
    }

    init(connection: IConnection, data: any) {
        console.log('MainState.init');
        this.connection = connection;
        this.entities = [];
        this.world.setBounds(data.x0, data.y0, data.width, data.height);
    }

    preload() {
        console.log('MainState.preload');
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
        this.game.debug.inputInfo(32, 40);
        this.game.debug.cameraInfo(this.game.camera, 300, 40);
    }

    onExit() {
        this.connection.stopGame();
    }

    getEntityById(id: string): IGameEntity {
        return this.entities[id];
    }

    follow = false;

    addEntity(re: IGameEntity) {
        //if (!this.follow) {
        //    this.follow = true;
        //    this.game.world.setBounds(-500, -500, 1000, 1000);
        //    //this.camera.setPosition(-200, -200);
        //    this.camera.follow(g, Phaser.Camera.FOLLOW_LOCKON);
        //    g.lineStyle(4, 0x0000FF);
        //}
        const g = this.createEntity(re);
        this.physics.enable(g, Phaser.Physics.ARCADE);
        g.body.velocity.x = re.dx;
        g.body.velocity.y = re.dy;

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
        this.game.debug.inputInfo(32, 32);
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
        console.log('loggedIn');
        this.showPage('game');
        this.start();
    };

    loggedOut() {
        console.log('loggedOut');
        this.stop();
        $('#login-form').trigger('reset');
        this.showPage('login');
    };

    logging() {
        console.log('logging');
        this.showPage('loading');
    };

    showPage(page: string) {
        $('#wrapper > div').hide();
        $(`#${page}-container`).show();
    }

    //#endregion
}
/// <reference path="../../typings/_references.ts" />

class StartState extends Phaser.State {
    create() {
        console.log('StartState.create');
    }

    init() {
        console.log('StartState.init');
        $('#play').on('click', () => this.game.state.start(GameState.main));
    }

    preload() {
        console.log('StartState.preload');
    }

    private entities: any = {};

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
        this.game.debug.inputInfo(32, 32);
    }

    getEntityById(id: string): IGameEntity {
        return this.entities[id];
    }

    addEntity(re: IGameEntity) {
        const g = this.add.graphics(re.x, re.y);
        g.lineStyle(2, 0x00FF00);
        g.drawRect(-5, -5, 10, 10);
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
}

class MainState extends Phaser.State {
    create() {
        console.log('MainState.create');
    }

    init() {
        console.log('MainState.init');
    }

    preload() {
        console.log('MainState.preload');
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
    public static data: IGameUpdate;

    constructor(connection: IConnection) {
        this.connection = connection;
        this.connection.onLoggedIn = () => this.loggedIn();
        this.connection.onLoggedOut = () => this.loggedOut();
        this.connection.onLogging = () => this.logging();

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
        this.game.state.start(GameState.start);
    }

    gameUpdate(data: IGameUpdate) {
        Game.data = data;
    }

    preload() {
    }

    create() {
        this.game.physics.startSystem(Phaser.Physics.ARCADE);
        //this.game.world.setBounds(-1000, -1000, 2000, 2000);
        this.game.add.plugin(Phaser.Plugin.Debug);
        this.game.stage.disableVisibilityChange = true;
        this.initStates();

        this.connection.startGame();
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
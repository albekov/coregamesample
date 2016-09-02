/// <reference path="../../typings/_references.ts" />

class StartState extends Phaser.State {
    create() {
        console.log('StartState.create');
    }

    init() {
        console.log('StartState.init');
    }

    preload() {
        console.log('StartState.preload');
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

class Game {
    private connection: IConnection;
    private game: Phaser.Game;

    constructor(connection: IConnection) {
        this.connection = connection;
        this.connection.onLoggedIn = () => this.loggedIn();
        this.connection.onLoggedOut = () => this.loggedOut();
        this.connection.onLogging = () => this.logging();

        this.initLoginEvents();
    }

    private start() {
        this.game = new Phaser.Game(600,
            400,
            Phaser.AUTO,
            'game',
            {
                preload: () => this.preload(),
                create: () => this.create()
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

    preload() {
    }

    create() {
        this.game.physics.startSystem(Phaser.Physics.ARCADE);
        this.game.world.setBounds(-1000, -1000, 2000, 2000);
        this.initStates();

        this.connection.game().start();
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
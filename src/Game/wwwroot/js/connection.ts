/// <reference path="../../typings/_references.ts" />

// ReSharper disable once InconsistentNaming
interface SignalR {
    authHub: IAuthHub;
    gameHub: any;
}

interface IAuthHubServer {
    init(authId: string): JQueryPromise<boolean>;
    login(username: string, password: string): JQueryPromise<string>;
    logout();
}

interface IAuthHubClient {
    message(msg: string): void;
    status(msg: string): void;
}

interface IAuthHub {
    server: IAuthHubServer;
    client: IAuthHubClient;
}

interface IConnection {
    onLoggedIn();
    onLoggedOut();
    onLogging();
    login(username, password);
    logout();

    onUpdate(data: any);
    startGame();
}

class Connection implements IConnection {
    private authHub: IAuthHub;
    private gameHub: any;
    onLoggedIn: () => void;
    onLoggedOut: () => void;
    onLogging: () => void;
    onUpdate: (data: any) => void;

    constructor() {
        this.authHub = $.connection.authHub;
        this.gameHub = $.connection.gameHub;
        $.connection.hub.logging = true;

        this.authHub.client.message = data => {
            console.log(`>> message: ${data}`);
        };

        this.gameHub.client.update = data => {
            this.onUpdate(data);
        };

        this.authHub.client.status = data => {
            $('#status').text(data);
        };
    }

    startGame() {
        this.gameHub.server.start();
    }

    connect() {
        $.connection.hub.start()
            .done(() => {
                this.init();
            });
    }

    private init() {
        this.onLogging();
        this.authHub.server
            .init(localStorage['authId'])
            .then(authId => this.handleLogin(authId));
    }

    login(username: string, password: string) {
        this.onLogging();
        this.authHub.server
            .login(username, password)
            .then(authId => this.handleLogin(authId));
    }

    private handleLogin(authId) {
        if (authId) {
            localStorage['authId'] = authId;
            this.onLoggedIn();
        } else {
            this.onLoggedOut();
        }
    }

    logout() {
        this.authHub.server.logout()
            .then(() => {
                delete localStorage['authId'];
                this.init();
            });
    }
}
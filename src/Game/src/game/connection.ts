import { Log } from '../utils';

declare var debug: boolean;

// ReSharper disable once InconsistentNaming
interface MySignalR extends SignalR {
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
}

export interface IAuthHub {
    server: IAuthHubServer;
    client: IAuthHubClient;
}

export interface IConnection {
    onLoggedIn();
    onLoggedOut();
    onLogging();
    login(username, password);
    logout();

    onStart(data: any);
    onStop();
    onUpdate(data: any);
    startGame();
    stopGame();

    moveTo(x, y: number);
}

export class Connection implements IConnection {
    private authHub: IAuthHub;
    private gameHub: any;
    onLoggedIn: () => void;
    onLoggedOut: () => void;
    onLogging: () => void;
    onStart: (data: any) => void;
    onStop: () => void;
    onUpdate: (data: any) => void;

    constructor() {
        const con = $.connection as MySignalR;
        this.authHub = con.authHub;
        this.gameHub = con.gameHub;
        con.hub.logging = debug;

        this.authHub.client.message = data => {
            Log.info(`Message: ${data}`);
        };

        this.gameHub.client.start = data => this.onStart(data);
        this.gameHub.client.stop = () => this.onStop();
        this.gameHub.client.update = data => this.onUpdate(data);
    }

    startGame() { this.gameHub.server.start(); }
    stopGame() { this.gameHub.server.stop(); }
    moveTo(x, y: number) { this.gameHub.server.moveTo(x, y); }

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
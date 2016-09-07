declare var debug: boolean;

export class Log {
    static info(...args) {
        if (debug)
            console.log.apply(console, args);
    }

    static warn(...args) {
        console.warn.apply(console, args);
    }
}
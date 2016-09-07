import { Connection } from './game/connection';
import { Game } from './game/game';

import './main.less';

const connection = new Connection();
const game = new Game(connection);
game.init();

connection.connect();
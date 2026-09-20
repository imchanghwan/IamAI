import { defineRoom, defineServer } from '@colyseus/core';
import { WebSocketTransport } from '@colyseus/ws-transport';
import { GameRoom } from './GameRoom.js';

const port = Number(process.env.PORT ?? 2567);

const server = defineServer({
  transport: new WebSocketTransport({ maxPayload: 4096 }),
  rooms: { game: defineRoom(GameRoom) },
});

await server.listen(port);

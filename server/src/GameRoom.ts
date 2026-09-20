import { Room, type Client } from '@colyseus/core';
import { schema, t, type SchemaType } from '@colyseus/schema';
import { checkStart, pickHost, type Phase } from './lobby.js';

const Member = schema({
  name: t.string(),
  isHost: t.boolean(),
});

const GameState = schema({
  phase: t.string().default('LOBBY'),
  hostId: t.string().default(''),
  members: t.map(Member),
});

export class GameRoom extends Room<{ state: SchemaType<typeof GameState> }> {
  override maxMessagesPerSecond = 20;

  override onCreate() {
    this.state = new GameState();
    this.onMessage('start', (client) => {
      const reason = checkStart(
        this.state.phase as Phase,
        this.state.hostId,
        client.sessionId,
        this.state.members.size,
      );
      if (reason) {
        client.send('startRejected', { reason });
        return;
      }
      this.state.phase = 'PLAYING';
    });
  }

  override onJoin(client: Client) {
    this.state.members.set(client.sessionId, new Member({ name: 'Player', isHost: false }));
    this.updateHost();
  }

  override onLeave(client: Client) {
    this.state.members.delete(client.sessionId);
    this.updateHost();
  }

  private updateHost() {
    const hostId = pickHost([...this.state.members.keys()]);
    this.state.hostId = hostId;
    this.state.members.forEach((m, id) => {
      m.isHost = id === hostId;
    });
  }
}

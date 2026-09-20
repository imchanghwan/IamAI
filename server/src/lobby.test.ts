import { describe, expect, it } from 'vitest';
import { MIN_PLAYERS_TO_START } from '@iamai/shared';
import { checkStart, pickHost } from './lobby.js';

describe('pickHost', () => {
  it('makes the first joiner host', () => {
    expect(pickHost(['a'])).toBe('a');
    expect(pickHost(['a', 'b'])).toBe('a');
  });

  it('promotes the next member when the host leaves', () => {
    expect(pickHost(['b', 'c'])).toBe('b');
  });

  it('has no host in an empty room', () => {
    expect(pickHost([])).toBe('');
  });
});

describe('checkStart', () => {
  const enough = MIN_PLAYERS_TO_START;

  it('accepts the host in LOBBY with enough players', () => {
    expect(checkStart('LOBBY', 'a', 'a', enough)).toBeNull();
  });

  it('rejects a non-host', () => {
    expect(checkStart('LOBBY', 'a', 'b', enough)).toBe('not_host');
  });

  it('rejects when under the minimum player count', () => {
    expect(checkStart('LOBBY', 'a', 'a', enough - 1)).toBe('not_enough_players');
  });

  it.each(['PLAYING', 'END'] as const)('rejects in phase %s', (phase) => {
    expect(checkStart(phase, 'a', 'a', enough)).toBe('wrong_phase');
  });
});

import { MIN_PLAYERS_TO_START } from '@iamai/shared';

export type Phase = 'LOBBY' | 'PLAYING' | 'END';

export type StartRejection = 'not_host' | 'wrong_phase' | 'not_enough_players';

// Join order is the order of `memberIds`; the earliest remaining member is host.
export function pickHost(memberIds: readonly string[]): string {
  return memberIds[0] ?? '';
}

export function checkStart(
  phase: Phase,
  hostId: string,
  requesterId: string,
  memberCount: number,
): StartRejection | null {
  if (phase !== 'LOBBY') return 'wrong_phase';
  if (requesterId !== hostId) return 'not_host';
  if (memberCount < MIN_PLAYERS_TO_START) return 'not_enough_players';
  return null;
}

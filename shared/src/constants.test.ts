import { describe, expect, it } from 'vitest';
import * as c from './constants.js';

describe('constants', () => {
  it('converts per-frame speeds to px/s with FRAME_RATE', () => {
    expect(c.MOVE_SPEED_PER_SEC).toBeCloseTo(132);
    expect(c.DASH_SPEED_PER_SEC).toBeCloseTo(252);
    expect(c.GHOST_SPEED_PER_SEC).toBeCloseTo(210);
  });

  it('orders speeds base < ghost < dash', () => {
    expect(c.MOVE_SPEED_PER_SEC).toBeLessThan(c.GHOST_SPEED_PER_SEC);
    expect(c.GHOST_SPEED_PER_SEC).toBeLessThan(c.DASH_SPEED_PER_SEC);
  });

  it('centers the zone on the world and starts it at half the diagonal', () => {
    expect(c.ZONE_CENTER_X * 2).toBe(c.WORLD_WIDTH);
    expect(c.ZONE_CENTER_Y * 2).toBe(c.WORLD_HEIGHT);
    expect(c.ZONE_INITIAL_RADIUS).toBeCloseTo(1500);
    expect(c.ZONE_FINAL_RADIUS).toBeLessThan(c.ZONE_INITIAL_RADIUS);
  });

  it('keeps cooldowns longer than their motions', () => {
    expect(c.ATTACK_COOLDOWN_SEC).toBeGreaterThan(c.ATTACK_MOTION_SEC);
    expect(c.DEFEND_COOLDOWN_SEC).toBeGreaterThan(c.DEFEND_DURATION_SEC);
  });

  it('patches no faster than the simulation ticks', () => {
    expect(c.PATCH_RATE_HZ).toBeLessThanOrEqual(c.SIM_TICK_RATE_HZ);
  });
});

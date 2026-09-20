export const WORLD_WIDTH = 2400;
export const WORLD_HEIGHT = 1800;
export const WORLD_EDGE_MARGIN = 20;

export const ROUND_DURATION_SEC = 180;
export const MIN_PLAYERS_TO_START = 2;
export const CLASSIC_BOT_COUNT = 27;
export const ENTITY_RADIUS = 12;
export const ROOM_CODE_LENGTH = 3;

export const SIM_TICK_RATE_HZ = 60;
export const PATCH_RATE_HZ = 20;

// The spec gives speeds per frame at an assumed 60 fps; the unit is not confirmed.
export const FRAME_RATE = 60;
export const MOVE_SPEED_PER_SEC = 2.2 * FRAME_RATE;
export const DASH_SPEED_PER_SEC = 4.2 * FRAME_RATE;
export const GHOST_SPEED_PER_SEC = 3.5 * FRAME_RATE;
// Per-frame factors (not per-second), kept as in the original.
export const ACCELERATION_FACTOR = 0.25;
export const FRICTION_FACTOR = 0.82;
export const STOP_THRESHOLD = 0.05;

export const STAMINA_MAX = 100;
export const STAMINA_DASH_DRAIN_PER_SEC = 40;
export const STAMINA_REGEN_PER_SEC = 25;
export const EXHAUSTION_DURATION_SEC = 2;

// Attack hit test: distance <= ATTACK_RANGE + target radius.
export const ATTACK_RANGE = 45;
export const ATTACK_HALF_ANGLE_DEG = 30;
export const ATTACK_COOLDOWN_SEC = 2;
export const ATTACK_MOTION_SEC = 0.3;
export const DEFEND_DURATION_SEC = 0.5;
export const DEFEND_COOLDOWN_SEC = 3;

export const ZONE_CENTER_X = WORLD_WIDTH / 2;
export const ZONE_CENTER_Y = WORLD_HEIGHT / 2;
export const ZONE_INITIAL_RADIUS = Math.hypot(WORLD_WIDTH, WORLD_HEIGHT) / 2;
export const ZONE_FINAL_RADIUS = 252;
export const ZONE_SHRINK_PER_SEC = 20;
// The zone starts shrinking only after the round timer reaches 0.

CREATE TABLE IF NOT EXISTS prototype_test (
    id          SERIAL PRIMARY KEY,
    title       TEXT        NOT NULL,
    is_done     BOOLEAN     NOT NULL DEFAULT FALSE,
    created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

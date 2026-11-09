DROP TABLE IF EXISTS app_logs;

CREATE TABLE IF NOT EXISTS log_req (
    id           BIGSERIAL PRIMARY KEY,
    request      TEXT,
    username     VARCHAR(100),
    created_by   VARCHAR(100) NOT NULL,   -- app name, from appsettings "AppName"
    created_date TIMESTAMPTZ  NOT NULL DEFAULT now()
);

CREATE TABLE IF NOT EXISTS log_resp (
    id               BIGSERIAL PRIMARY KEY,
    request_id       BIGINT REFERENCES log_req (id),
    log_level        VARCHAR(20)  NOT NULL,   -- INFO / ERROR
    category         VARCHAR(40),             -- error classification, null on success
    source           VARCHAR(200) NOT NULL,   -- e.g. "PrototypeBusinessLogic.CreateAsync"
    message          TEXT,
    exception_detail TEXT,                    -- ex.ToString(), null on success
    created_at       TIMESTAMPTZ  NOT NULL DEFAULT now()
);

CREATE INDEX IF NOT EXISTS idx_log_req_created_date ON log_req (created_date);
CREATE INDEX IF NOT EXISTS idx_log_resp_request_id  ON log_resp (request_id);
CREATE INDEX IF NOT EXISTS idx_log_resp_created_at  ON log_resp (created_at);
CREATE INDEX IF NOT EXISTS idx_log_resp_category    ON log_resp (category);

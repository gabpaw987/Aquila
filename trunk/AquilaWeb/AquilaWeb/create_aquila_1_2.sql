\c template1
DROP DATABASE aquila;
CREATE DATABASE aquila;
\c aquila

-- PostgreSQL 8 Membership Provider Schema

CREATE TABLE "Users" (
    "pId"                                       character(36)           NOT NULL,
    "Username"                                  character varying(255)  NOT NULL,
    "ApplicationName"                           character varying(255)  NOT NULL,
    "Email"                                     character varying(128)  NULL,
    "TelNr"                                     character varying(128)  NULL,
    "Comment"                                   character varying(128)  NULL,
    "Password"                                  character varying(255)  NOT NULL,
    "PasswordQuestion"                          character varying(255)  NULL,
    "PasswordAnswer"                            character varying(255)  NULL,
    "IsApproved"                                boolean                 NULL, 
    "LastActivityDate"                          timestamptz             NULL,
    "LastLoginDate"                             timestamptz             NULL,
    "LastPasswordChangedDate"                   timestamptz             NULL,
    "CreationDate"                              timestamptz             NULL, 
    "IsOnLine"                                  boolean                 NULL,
    "IsLockedOut"                               boolean                 NULL,
    "LastLockedOutDate"                         timestamptz             NULL,
    "FailedPasswordAttemptCount"                integer                 NULL,
    "FailedPasswordAttemptWindowStart"          timestamptz             NULL,
    "FailedPasswordAnswerAttemptCount"          integer                 NULL,
    "FailedPasswordAnswerAttemptWindowStart"    timestamptz             NULL,
    CONSTRAINT users_pkey PRIMARY KEY ("pId"),
    CONSTRAINT users_username_application_unique UNIQUE ("Username", "ApplicationName")
);

CREATE INDEX users_email_index ON "Users" ("Email");
CREATE INDEX users_islockedout_index ON "Users" ("IsLockedOut");

-- PostgreSQL 8 Role Provider Schema

CREATE TABLE "Roles" (
    "Rolename"              character varying(255)  NOT NULL,
    "ApplicationName"       character varying(255)  NOT NULL,
    CONSTRAINT roles_pkey PRIMARY KEY ("Rolename", "ApplicationName")
);

CREATE TABLE "UsersInRoles" (
    "Username"              character varying(255)  NOT NULL,
    "Rolename"              character varying(255)  NOT NULL,
    "ApplicationName"       character varying(255)  NOT NULL,
    CONSTRAINT usersinroles_pkey PRIMARY KEY ("Username", "Rolename", "ApplicationName"),
    CONSTRAINT usersinroles_username_fkey FOREIGN KEY ("Username", "ApplicationName") REFERENCES "Users" ("Username", "ApplicationName") ON DELETE CASCADE,
    CONSTRAINT usersinroles_rolename_fkey FOREIGN KEY ("Rolename", "ApplicationName") REFERENCES "Roles" ("Rolename", "ApplicationName") ON DELETE CASCADE
);

-- PostgreSQL 8 Profile Provider Schema

CREATE TABLE "Profiles" (
    "pId"                   character(36)           NOT NULL,
    "Username"              character varying(255)  NOT NULL,
    "ApplicationName"       character varying(255)  NOT NULL,
    "IsAnonymous"           boolean                 NULL,
    "LastActivityDate"      timestamptz             NULL,
    "LastUpdatedDate"       timestamptz             NULL,
    CONSTRAINT profiles_pkey PRIMARY KEY ("pId"),
    CONSTRAINT profiles_username_application_unique UNIQUE ("Username", "ApplicationName"),
    CONSTRAINT profiles_username_fkey FOREIGN KEY ("Username", "ApplicationName") REFERENCES "Users" ("Username", "ApplicationName") ON DELETE CASCADE
);

CREATE INDEX profiles_isanonymous_index ON "Profiles" ("IsAnonymous");

CREATE TABLE "ProfileData" (
    "pId"                   character(36)           NOT NULL,
    "Profile"               character(36)           NOT NULL,
    "Name"                  character varying(255)  NOT NULL,
    "ValueString"           text                    NULL,
    "ValueBinary"           bytea                   NULL,
    CONSTRAINT profiledata_pkey PRIMARY KEY ("pId"),
    CONSTRAINT profiledata_profile_name_unique UNIQUE ("Profile", "Name"),
    CONSTRAINT profiledata_profile_fkey FOREIGN KEY ("Profile") REFERENCES "Profiles" ("pId") ON DELETE CASCADE
);

-- PostgreSQL 8 Session-Store Provider Schema

CREATE TABLE "Sessions" (
    "SessionId"             character varying(80)   NOT NULL,
    "ApplicationName"       character varying(255)  NOT NULL,
    "Created"               timestamptz             NOT NULL,
    "Expires"               timestamptz             NOT NULL,
    "Timeout"               integer                 NOT NULL,
    "Locked"                boolean                 NOT NULL,
    "LockId"                integer                 NOT NULL,
    "LockDate"              timestamptz             NOT NULL,
    "Data"                  text                    NULL,
    "Flags"                 integer                 NOT NULL,
    CONSTRAINT sessions_pkey PRIMARY KEY ("SessionId", "ApplicationName")
);

-- DROP TABLE IF EXISTS portfolio CASCADE;

CREATE TABLE portfolio (
    pfid          SERIAL,
    capital       DECIMAL(14,2) CONSTRAINT positive_capital CHECK (capital > 0),
	-- cash at risk; invested (exposure)
	invested      DECIMAL(14,2),
    ytdreturn     DECIMAL(12,2),
	urpf          DECIMAL(12,2),
	rpf           DECIMAL(12,2),
    PRIMARY KEY(pfid)
);

INSERT INTO portfolio (capital) VALUES (500000);

-- DROP TABLE IF EXISTS pfcontrol CASCADE;
CREATE TABLE pfcontrol (
    pfid          INT,
    "pId"         VARCHAR(255),
    PRIMARY KEY (pfid, "pId"),
    FOREIGN KEY (pfid)    REFERENCES portfolio (pfid)
      ON UPDATE CASCADE
      ON DELETE CASCADE,
    FOREIGN KEY ("pId")  REFERENCES "Users" ("pId")
      ON UPDATE CASCADE
      ON DELETE CASCADE
);

-- DROP TABLE IF EXISTS exchange CASCADE;
CREATE TABLE exchange (
    ename         VARCHAR(255),
    country       VARCHAR(255),
    PRIMARY KEY (ename)
);

INSERT INTO exchange (ename, country) VALUES ('NYSE', 'USA');
INSERT INTO exchange (ename, country) VALUES ('NASDAQ GS', 'USA');

-- DROP TABLE IF EXISTS currency CASCADE;
CREATE TABLE currency (
    currency      VARCHAR(20),
    PRIMARY KEY (currency)
);

INSERT INTO currency (currency) VALUES ('EUR');
INSERT INTO currency (currency) VALUES ('USD');
INSERT INTO currency (currency) VALUES ('GBP');

-- DROP TABLE IF EXISTS series CASCADE;
CREATE TABLE series (
    symbol        VARCHAR(30),
    currency      VARCHAR(20),
    ename         VARCHAR(255),
    tradeable     BOOLEAN CONSTRAINT not_null_tradeable CHECK (tradeable IS NOT NULL),
	decision      INT,
    addinfo       VARCHAR(255),
    PRIMARY KEY (symbol),
    FOREIGN KEY (currency) REFERENCES currency (currency)
      ON UPDATE CASCADE
      ON DELETE SET NULL,
    FOREIGN KEY (ename)  REFERENCES exchange (ename)
      ON UPDATE CASCADE
      ON DELETE CASCADE
);

INSERT INTO series (symbol, currency, ename, tradeable, decision, addinfo) VALUES ('INDU:IND', 'USD', 'NYSE', false, NULL, '');
INSERT INTO series (symbol, currency, ename, tradeable, decision, addinfo) VALUES ('USD-EUR', 'USD', NULL, false, NULL, '');
INSERT INTO series (symbol, currency, ename, tradeable, decision, addinfo) VALUES ('AAPL:US', 'USD', 'NASDAQ GS', true, NULL, '');

-- DROP TABLE IF EXISTS stock CASCADE;
CREATE TABLE stock (
    symbol        VARCHAR(30),
    PRIMARY KEY (symbol),
    FOREIGN KEY (symbol) REFERENCES series (symbol)
      ON UPDATE CASCADE
      ON DELETE CASCADE
);

INSERT INTO stock (symbol) VALUES ('AAPL:US');

-- DROP TABLE IF EXISTS indexdata CASCADE;
CREATE TABLE indexdata (
    symbol        VARCHAR(30),
    PRIMARY KEY (symbol),
    FOREIGN KEY (symbol) REFERENCES series (symbol)
      ON UPDATE CASCADE
      ON DELETE CASCADE
);

INSERT INTO indexdata (symbol) VALUES ('INDU:IND');
INSERT INTO indexdata (symbol) VALUES ('USD-EUR');

-- DROP TABLE IF EXISTS sindex CASCADE;
CREATE TABLE sindex (
    symbol        VARCHAR(30),
    nrstocks      INT,
    PRIMARY KEY (symbol),
    FOREIGN KEY (symbol) REFERENCES indexdata (symbol)
      ON UPDATE CASCADE
      ON DELETE CASCADE
);

INSERT INTO sindex (symbol, nrstocks) VALUES ('INDU:IND', 30);

-- DROP TABLE IF EXISTS sindex CASCADE;
CREATE TABLE forex (
    symbol        VARCHAR(30),
    fcurrency     VARCHAR(20),
    PRIMARY KEY (symbol),
    FOREIGN KEY (symbol) REFERENCES indexdata (symbol)
      ON UPDATE CASCADE
      ON DELETE CASCADE
);

INSERT INTO forex (symbol, fcurrency) VALUES ('USD-EUR', 'EUR');

-- DROP TABLE IF EXISTS future CASCADE;
CREATE TABLE future (
    symbol        VARCHAR(30),
    multiplier    DECIMAL(12,4),
    underlying    VARCHAR(30),
    maturity      DATE,
    PRIMARY KEY (symbol),
    FOREIGN KEY (symbol) REFERENCES series (symbol)
      ON UPDATE CASCADE
      ON DELETE CASCADE,
    FOREIGN KEY (underlying) REFERENCES series (symbol)
      ON UPDATE CASCADE
      ON DELETE CASCADE
);

-- DROP TABLE IF EXISTS soption CASCADE;
CREATE TABLE soption (
    symbol        VARCHAR(30),
    multiplier    DECIMAL(12,4),
    underlying    VARCHAR(30),
    put           BOOLEAN,
    strike        DECIMAL(13,5),
    maturity      DATE,
    PRIMARY KEY (symbol),
    FOREIGN KEY (symbol) REFERENCES series (symbol)
      ON UPDATE CASCADE
      ON DELETE CASCADE,
    FOREIGN KEY (underlying) REFERENCES series (symbol)
      ON UPDATE CASCADE
      ON DELETE CASCADE
);

-- DROP TABLE IF EXISTS etf CASCADE;
CREATE TABLE etf (
    symbol        VARCHAR(30),
    maturity      DATE,
    PRIMARY KEY (symbol),
    FOREIGN KEY (symbol) REFERENCES series (symbol)
      ON UPDATE CASCADE
      ON DELETE CASCADE
);

-- DROP TABLE IF EXISTS bond CASCADE;
CREATE TABLE bond (
    symbol        VARCHAR(30),
    duration      INTERVAL,
    maturity      DATE,
    -- fixed interest
    fi            BOOLEAN,
    -- interest rate
    irate         DECIMAL(20,14),
    PRIMARY KEY (symbol),
    FOREIGN KEY (symbol) REFERENCES series (symbol)
      ON UPDATE CASCADE
      ON DELETE CASCADE
);

-- DROP TABLE IF EXISTS pfsecurity CASCADE;
CREATE TABLE pfsecurity (
    pfid          INT,
    symbol        VARCHAR(30),
    position      INT,
	gain          DECIMAL(13,5),
	maxinvest     DECIMAL(10,2),
    cutloss       DECIMAL(7,4),
	roi           DECIMAL(10,4),
	-- automatic/manual
	auto          BOOLEAN,
	active        BOOLEAN DEFAULT false,
    PRIMARY KEY (pfid, symbol),
    FOREIGN KEY (pfid)    REFERENCES portfolio (pfid)
      ON UPDATE CASCADE
      ON DELETE CASCADE,
    FOREIGN KEY (symbol) REFERENCES series (symbol)
      ON UPDATE CASCADE
      ON DELETE CASCADE
);

INSERT INTO pfsecurity (pfid, symbol, position, maxinvest, cutloss) VALUES (1, 'AAPL:US', '5', '10000', '8.0');

-- DROP TABLE IF EXISTS advising CASCADE;
CREATE TABLE advising (
    "pId"         VARCHAR(255),
    symbol        VARCHAR(30),
    PRIMARY KEY ("pId", symbol),
    FOREIGN KEY ("pId")  REFERENCES "Users" ("pId")
      ON UPDATE CASCADE
      ON DELETE CASCADE,
    FOREIGN KEY (symbol) REFERENCES series (symbol)
      ON UPDATE CASCADE
      ON DELETE CASCADE
);

-- DROP TABLE IF EXISTS ordertype CASCADE;
CREATE TABLE ordertype (
    oname         VARCHAR(255),
    PRIMARY KEY(oname)
);

INSERT INTO ordertype (oname) VALUES ('limit');
INSERT INTO ordertype (oname) VALUES ('market');

-- DROP TABLE IF EXISTS sorder CASCADE;
CREATE TABLE sorder (
    pfid           INT,
    symbol        VARCHAR(30),
    -- time entered
    ten           TIMESTAMP WITH TIME ZONE,
    -- time executed
    tex           TIMESTAMP WITH TIME ZONE,
    oname         VARCHAR(255),
    -- price entered
    priceen       DECIMAL(13,5),
    -- price executed
    priceex       DECIMAL(13,5),
    osize         INT,
    executed      BOOLEAN,
    fee           DECIMAL(13,5),
    PRIMARY KEY (pfid, symbol, ten),
    FOREIGN KEY (pfid)  REFERENCES portfolio (pfid)
      ON UPDATE CASCADE
      ON DELETE CASCADE,
    FOREIGN KEY (symbol)       REFERENCES series (symbol)
      ON UPDATE CASCADE
      ON DELETE CASCADE,
    FOREIGN KEY (oname)        REFERENCES ordertype (oname)
      ON UPDATE CASCADE
      ON DELETE SET NULL
);

INSERT INTO sorder (pfid, symbol,    ten,                                      tex,               oname,   priceen, priceex, osize, executed, fee)
            VALUES (1,   'AAPL:US', current_timestamp - INTERVAL '5 seconds', current_timestamp, 'limit', 575.900, 575.860, 5,     true,     1.0);

-- Minute Bar
-- DROP TABLE IF EXISTS mbar CASCADE;
CREATE TABLE mbar (
    symbol        VARCHAR(30),
    t             TIMESTAMP WITH TIME ZONE,
    -- open
    o             DECIMAL(13,5),
    -- high
    h             DECIMAL(13,5),
    -- low
    l             DECIMAL(13,5),
    -- close
    c             DECIMAL(13,5),
    -- volume
    v             INT,
    PRIMARY KEY (symbol, t),
    FOREIGN KEY (symbol)  REFERENCES series (symbol)
      ON UPDATE CASCADE
      ON DELETE CASCADE
);

INSERT INTO mbar (symbol, t, o, h, l, c, v) VALUES ('AAPL:US', current_timestamp - INTERVAL '5 seconds', 575.000, 575.900, 575.000, 575.850, 19926056);

-- DROP TABLE IF EXISTS signaltype CASCADE;
CREATE TABLE signaltype (
    -- e.g. 0, -1, 1
    sval          INT,
    sname         VARCHAR(255),
    -- e.g. 'hold', 'sell', 'buy'
    PRIMARY KEY (sval)
);

INSERT INTO signaltype (sval, sname) VALUES (0, 'hold');
INSERT INTO signaltype (sval, sname) VALUES (1, 'buy');
INSERT INTO signaltype (sval, sname) VALUES (-1, 'sell');

-- DROP TABLE IF EXISTS signal CASCADE;
CREATE TABLE signal (
    symbol        VARCHAR(30),
    t             TIMESTAMP WITH TIME ZONE,
    sval          INT,
    PRIMARY KEY (symbol, t),
    FOREIGN KEY (symbol)  REFERENCES series (symbol)
      ON UPDATE CASCADE
      ON DELETE CASCADE,
    FOREIGN KEY (sval)    REFERENCES signaltype (sval)
      ON UPDATE CASCADE
      ON DELETE SET NULL
);

-- DROP TABLE IF EXISTS dbar CASCADE;
CREATE TABLE dbar (
    symbol        VARCHAR(30),
    bdate         DATE,
    o             DECIMAL(13,5),
    h             DECIMAL(13,5),
    l             DECIMAL(13,5),
    c             DECIMAL(13,5),
    v             INT,
    PRIMARY KEY (symbol, bdate),
    FOREIGN KEY (symbol)  REFERENCES series (symbol)
      ON UPDATE CASCADE
      ON DELETE CASCADE
);

-- DROP TABLE IF EXISTS indicatortype CASCADE;
CREATE TABLE indicatortype (
    iname         VARCHAR(255),
    PRIMARY KEY(iname)
);

INSERT INTO indicatortype (iname) VALUES ('SMA');
INSERT INTO indicatortype (iname) VALUES ('LWMA');
INSERT INTO indicatortype (iname) VALUES ('EMA');
INSERT INTO indicatortype (iname) VALUES ('DEMA');
INSERT INTO indicatortype (iname) VALUES ('TEMA');
INSERT INTO indicatortype (iname) VALUES ('RSI');
INSERT INTO indicatortype (iname) VALUES ('CCI');
INSERT INTO indicatortype (iname) VALUES ('Aroon Up');
INSERT INTO indicatortype (iname) VALUES ('Aroon Down');

-- DROP TABLE IF EXISTS indicator CASCADE;
CREATE TABLE indicator (
    iname         VARCHAR(255),
    ilength       INTERVAL,
    PRIMARY KEY (iname),
    FOREIGN KEY (iname)  REFERENCES indicatortype (iname)
      ON UPDATE CASCADE
      ON DELETE CASCADE
);

CREATE TABLE indicatorval (
    symbol        VARCHAR(30),
    iname         VARCHAR(255),
    t             TIMESTAMP WITH TIME ZONE,
    ival          DECIMAL(13,5),
    PRIMARY KEY (symbol),
    FOREIGN KEY (iname) REFERENCES indicator (iname)
      ON UPDATE CASCADE
      ON DELETE CASCADE,
    FOREIGN KEY (symbol) REFERENCES series (symbol)
      ON UPDATE CASCADE
      ON DELETE CASCADE
);


-- TRIGGER

-- Only Uppercase Symbols
CREATE OR REPLACE FUNCTION upper_symbol() RETURNS TRIGGER AS $upper_trigger$
	BEGIN
		NEW.symbol := upper(NEW.symbol);
		RETURN NEW;
	END;
$upper_trigger$ LANGUAGE plpgsql;

CREATE TRIGGER series_insert_symbol
BEFORE INSERT ON series
FOR EACH ROW
EXECUTE PROCEDURE upper_symbol();
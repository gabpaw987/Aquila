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

-- no more enums
--CREATE TYPE bsizetypes AS ENUM ('mBar', 'dBar');
--CREATE TYPE btypetype AS ENUM ('Ask', 'Last', 'Trades', 'Bid', 'Midpoint');

-- DROP TABLE IF EXISTS portfolio CASCADE;
CREATE TABLE portfolio (
    pfid          SERIAL,
    capital       DECIMAL(14,2) CONSTRAINT positive_capital CHECK (capital > 0),
	-- cash at risk; invested (exposure)
	invested      DECIMAL(14,2) DEFAULT 0,
    ytdreturn     DECIMAL(12,2) DEFAULT 0,
	urpf          DECIMAL(12,2) DEFAULT 0,
	rpf           DECIMAL(12,2) DEFAULT 0,
	-- price premium percentage
	ppp           DECIMAL(5,2),
	-- bar size (mBar, dBar)
	bsize         VARCHAR CHECK(bsize IN ('mBar', 'dBar')),
	-- bar type (Ask, Last/Trades, Bid, Midpoint)
	btype         VARCHAR CHECK(btype IN ('Ask', 'Last', 'Trades', 'Bid', 'Midpoint')),
    PRIMARY KEY(pfid)
);

INSERT INTO portfolio (capital, invested, ytdreturn, urpf, rpf, ppp, bsize, btype) VALUES (500000, 0, 0, 0,0, 1, 'dBar', 'Last');

-- DROP TABLE IF EXISTS profithistory CASCADE;
CREATE TABLE profithistory (
    pfid          SERIAL,
    t             TIMESTAMP WITH TIME ZONE,
	rpf           DECIMAL(12,2) DEFAULT 0,
	PRIMARY KEY(t)
);

INSERT INTO profithistory(pfid, t, rpf) VALUES(1, now(), 0);

-- insert values into profit history
CREATE OR REPLACE FUNCTION profit_history() RETURNS TRIGGER AS $profit_history$
	BEGIN
		INSERT INTO profithistory(pfid, t, rpf)
		VALUES(
			NEW.pfid,
			now(),
			(SELECT rpf FROM profithistory ORDER BY t DESC LIMIT 1) + NEW.rpf
		);

		RETURN NEW;
	END;
$profit_history$ LANGUAGE plpgsql;

CREATE TRIGGER update_profit_history
AFTER UPDATE ON portfolio
FOR EACH ROW
EXECUTE PROCEDURE profit_history();

-- DROP TABLE IF EXISTS pfcontrol CASCADE;
CREATE TABLE pfcontrol (
    pfid          INT,
    "pId"         VARCHAR(255),
	maxinvest     DECIMAL(10,2),
    cutloss       DECIMAL(7,4) DEFAULT 5,
	auto          BOOLEAN,
	notify		  BOOLEAN DEFAULT false,
    PRIMARY KEY (pfid, "pId"),
    FOREIGN KEY (pfid)    REFERENCES portfolio (pfid)
      ON UPDATE CASCADE
      ON DELETE CASCADE,
    FOREIGN KEY ("pId")  REFERENCES "Users" ("pId")
      ON UPDATE CASCADE
      ON DELETE CASCADE
);

INSERT INTO pfcontrol(pfid, "pId", maxinvest, cutloss, auto) VALUES(1, (SELECT "pId" FROM "Users" LIMIT 1), 10000, 4, false);

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
	rsssymbol     VARCHAR(30),
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

INSERT INTO series (symbol, rsssymbol, currency, ename, tradeable, decision, addinfo) VALUES ('INDU:IND', '^DJI', 'USD', 'NYSE', false, NULL, '');
INSERT INTO series (symbol, rsssymbol, currency, ename, tradeable, decision, addinfo) VALUES ('USD-EUR', 'USDEUR=X', 'USD', NULL, false, NULL, '');
INSERT INTO series (symbol, rsssymbol, currency, ename, tradeable, decision, addinfo) VALUES ('AAPL:US', 'AAPL', 'USD', 'NASDAQ GS', true, NULL, '');

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
    position      INT  DEFAULT 0,
	-- unrealised
	gain          DECIMAL(13,5) DEFAULT 0,
	-- realised
	rgain		  DECIMAL(13,5) DEFAULT 0,
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

DELETE FROM sorder;
UPDATE pfsecurity SET gain = 0;
UPDATE pfsecurity SET rgain = 0;
UPDATE pfsecurity SET position = 0;
--UPDATE portfolio SET rpf = 0;
--UPDATE portfolio SET urpf = 0;
--UPDATE portfolio SET invested = 0;

INSERT INTO sorder (pfid, symbol, ten, tex, oname, priceen, priceex, osize, executed, fee)
VALUES (1, 'AAPL:US', current_timestamp - INTERVAL '31 seconds', current_timestamp - INTERVAL '30 seconds', 'limit', 599.900, 600.000, 5, true, 1.0);

SELECT * FROM pfsecurity;

INSERT INTO sorder (pfid, symbol, ten, tex, oname, priceen, priceex, osize, executed, fee)
VALUES (1, 'AAPL:US', current_timestamp - INTERVAL '21 seconds', current_timestamp - INTERVAL '20 seconds', 'limit', 609.900, 600.400, -5, true, 1.0);

SELECT * FROM pfsecurity;

INSERT INTO sorder (pfid, symbol, ten, tex, oname, priceen, priceex, osize, executed, fee)
VALUES (1, 'AAPL:US', current_timestamp - INTERVAL '16 seconds', current_timestamp - INTERVAL '15 seconds', 'limit', 599.900, 600.000, 4, true, 1.0);
	
SELECT * FROM pfsecurity;

INSERT INTO sorder (pfid, symbol, ten, tex, oname, priceen, priceex, osize, executed, fee)
VALUES (1, 'AAPL:US', current_timestamp - INTERVAL '11 seconds', current_timestamp - INTERVAL '10 seconds', 'limit', 609.900, 610.000, -3, true, 1.0);

SELECT * FROM pfsecurity;

INSERT INTO sorder (pfid, symbol, ten, tex, oname, priceen, priceex, osize, executed, fee)
VALUES (1, 'AAPL:US', current_timestamp - INTERVAL '6 seconds', current_timestamp - INTERVAL '5 seconds', 'limit', 609.900, 610.000, -1, true, 1.0);	

SELECT * FROM pfsecurity;

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

INSERT INTO dbar VALUES('AAPL:US',current_date,590,620,590,610,0);

-- DROP TABLE IF EXISTS indicator CASCADE;
CREATE TABLE indicator (
    iname         VARCHAR(255),
    ilength       INTERVAL,
    PRIMARY KEY (iname)
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

-----------------------------------------------------------------
-- Orders change position and gain:
-----------------------------------------------------------------

SELECT c
FROM(
	SELECT t, c
	FROM mbar
	WHERE t=(
		SELECT max(t)
		FROM mbar
		WHERE symbol='AAPL:US'
	)
	AND symbol='AAPL:US'
	
	UNION
	
	SELECT bdate, c
	FROM dbar
	WHERE bdate=(
		SELECT max(bdate)
		FROM dbar
		WHERE symbol='AAPL:US'
	)
	AND symbol='AAPL:US'
)AS temp1
ORDER BY t DESC
LIMIT 1;

CREATE OR REPLACE FUNCTION get_latest_bar (VARCHAR) RETURNS DECIMAL AS $get_latest_bar$
	DECLARE
		ret DECIMAL;
	BEGIN
		SELECT c INTO ret
		FROM(
			SELECT t, c
			FROM mbar
			WHERE t=(
				SELECT max(t)
				FROM mbar
				WHERE symbol=$1
			)
			AND symbol=$1
	
			UNION
	
			SELECT bdate, c
			FROM dbar
			WHERE bdate=(
				SELECT max(bdate)
				FROM dbar
				WHERE symbol=$1
			)
			AND symbol=$1
		)AS temp1
		ORDER BY t DESC
		LIMIT 1;
		RETURN ret;
	END;
$get_latest_bar$ LANGUAGE plpgsql;

SELECT * FROM get_latest_bar('AAPL:US');

CREATE OR REPLACE FUNCTION get_pos_buying_price (INTEGER, VARCHAR) RETURNS DECIMAL AS $get_pos_buying_price$
	DECLARE
		pos INTEGER;
		price DECIMAL := 0;
		v RECORD;
	BEGIN
		-- current position
		SELECT position INTO pos
		FROM pfsecurity
		WHERE symbol = $2
		AND pfid = $1;

		-- loop thru orders with different signs
		FOR v IN (SELECT osize, priceex FROM sorder WHERE sign(osize)=sign(pos) ORDER BY tex DESC) LOOP
			IF (abs(pos) <= v.osize) THEN
				RETURN price + pos*v.priceex;
			ELSE
				price := price + v.osize*v.priceex;
				IF (pos > 0) THEN
					pos := pos - v.osize;
				ELSE
					pos := pos + v.osize;
				END IF;
			END IF;
		END LOOP;
		RETURN 0;
	END;
$get_pos_buying_price$ LANGUAGE plpgsql;

SELECT * FROM get_pos_buying_price(1, 'AAPL:US');

CREATE OR REPLACE FUNCTION order_aggregate_insert() RETURNS TRIGGER AS $position_aggregate_insert$
	DECLARE
		pos_old INTEGER;
		pos_new INTEGER;
		liq_size INTEGER;
		cash DECIMAL;
		unrealised DECIMAL;
	BEGIN
		IF (NEW.executed = true) THEN
			-- get old position
			SELECT position INTO pos_old
			FROM pfsecurity
			WHERE symbol = NEW.symbol
			AND pfid = NEW.pfid;

			-- new position
			pos_new := pos_old + NEW.osize;

			-- update position
			UPDATE pfsecurity
			SET position = pos_new
			WHERE symbol = NEW.symbol
			AND pfid = NEW.pfid;

			-- cash:
			-- revenues minus spendings
			SELECT (coalesce(sum(-1*osize*priceex - fee),0) + (-1*NEW.osize*NEW.priceex - NEW.fee)) INTO cash
			FROM sorder
			WHERE symbol = NEW.symbol
			AND pfid = NEW.pfid;

			--RAISE NOTICE '%', (SELECT sum(-1*osize*priceex - fee) FROM sorder);
			--RAISE NOTICE '%', (-1*NEW.osize*NEW.priceex - NEW.fee);
			--RAISE NOTICE '%', (SELECT (sum(-1*osize*priceex - fee) + (-1*NEW.osize*NEW.priceex - NEW.fee)) FROM sorder);

			-- closing of positions -> realised gain/loss
			IF (abs(pos_new) < abs(pos_old)) THEN
				IF (abs(pos_old - pos_new) < abs(pos_old)) THEN
					liq_size := abs(pos_old - pos_new);
				ELSE
					liq_size := abs(pos_old);
				END IF;

				-- realised profit
				UPDATE pfsecurity
				SET rgain = cash + get_pos_buying_price(NEW.pfid, NEW.symbol)
				WHERE symbol = NEW.symbol
				AND pfid = NEW.pfid;
			END IF;

			-- unrealised profit:
			SELECT pos_new * 
			-- newest closing price (dbar or mbar)
			(SELECT * FROM get_latest_bar(NEW.symbol)) INTO unrealised;

			-- update unrealised profit
			UPDATE pfsecurity
			SET gain = cash + unrealised
			WHERE symbol = NEW.symbol
			AND pfid = NEW.pfid;

			--RAISE NOTICE '% %', cash, unrealised;

		END IF;
		RETURN NEW;
	END;
$position_aggregate_insert$ LANGUAGE plpgsql;

CREATE TRIGGER sorder_insert
BEFORE INSERT ON sorder
FOR EACH ROW
EXECUTE PROCEDURE order_aggregate_insert();

-----------------------------------------------------------------
-- Gain changes rpf (realised profit) and urpf (unrealised profit):
-----------------------------------------------------------------

CREATE OR REPLACE FUNCTION gain_aggregate_update() RETURNS TRIGGER AS $gain_aggregate_update$
	BEGIN
		UPDATE portfolio
		SET rpf = rpf - OLD.rgain + NEW.rgain,
			urpf = urpf - OLD.gain + NEW.gain
		WHERE pfid = NEW.pfid;

		RETURN NEW;
	END;
$gain_aggregate_update$ LANGUAGE plpgsql;

CREATE TRIGGER gain_update
BEFORE UPDATE ON pfsecurity
FOR EACH ROW
EXECUTE PROCEDURE gain_aggregate_update();

-----------------------------------------------------------------
-- Aggregate invested:
-----------------------------------------------------------------

SELECT * FROM get_pos_buying_price(1, 'AAPL:US');

CREATE OR REPLACE FUNCTION update_invested() RETURNS TRIGGER AS $update_invested$
	BEGIN
		-- RAISE NOTICE '%', (SELECT * FROM get_pos_buying_price(NEW.pfid, NEW.symbol));
		IF NEW.executed = true THEN
			UPDATE portfolio
			SET invested = (
				SELECT *
				FROM get_pos_buying_price(NEW.pfid, NEW.symbol)
			);
		END IF;

		RETURN NEW;
	END;
$update_invested$ LANGUAGE plpgsql;

CREATE TRIGGER sorder_invested_after_insert
AFTER INSERT ON sorder
FOR EACH ROW
EXECUTE PROCEDURE update_invested();

-----------------------------------------------------------------
-- Only Uppercase Symbols:
-----------------------------------------------------------------

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

CREATE TRIGGER series_update_symbol
BEFORE UPDATE ON series
FOR EACH ROW
EXECUTE PROCEDURE upper_symbol();

-----------------------------------------------------------------
-- Standard values for maxinvest, cutloss and auto in pfsecurity:
-----------------------------------------------------------------

CREATE OR REPLACE FUNCTION std_values_pfsecurity() RETURNS TRIGGER AS $std_values_pfsecurity$
	BEGIN
		IF (NEW.maxinvest IS NULL) THEN
			SELECT maxinvest INTO NEW.maxinvest
			FROM pfcontrol
			WHERE pfid = NEW.pfid;
		END IF;
		IF (NEW.cutloss IS NULL) THEN
			SELECT cutloss INTO NEW.cutloss
			FROM pfcontrol
			WHERE pfid = NEW.pfid;
		END IF;
		IF (NEW.auto IS NULL) THEN
			SELECT auto INTO NEW.auto
			FROM pfcontrol
			WHERE pfid = NEW.pfid;
		END IF;
		RETURN NEW;
	END;
$std_values_pfsecurity$ LANGUAGE plpgsql;

CREATE TRIGGER pfsecurity_insert_std_values
BEFORE INSERT ON pfsecurity
FOR EACH ROW
EXECUTE PROCEDURE std_values_pfsecurity();

-----------------------------------------------------------------
-- Portfolio capital > sum of maximum individual investments:
-----------------------------------------------------------------

-- INSERT maxinvest
CREATE OR REPLACE FUNCTION investment_cap_insert() RETURNS TRIGGER AS $investment_cap$
	BEGIN
		-- not enough capital
		IF ((SELECT capital FROM portfolio WHERE pfid=NEW.pfid) <
		((SELECT sum(maxinvest) FROM pfsecurity WHERE pfid=NEW.pfid)+NEW.maxinvest)) THEN
			RAISE EXCEPTION 'Portfolio capital too small for this maximum investment.';
		END IF;
		RETURN NEW;
	END;
$investment_cap$ LANGUAGE plpgsql;

CREATE TRIGGER pfsecurity_insert_maxinvest
BEFORE INSERT ON pfsecurity
FOR EACH ROW
EXECUTE PROCEDURE investment_cap_insert();

-- UPDATE maxinvest
CREATE OR REPLACE FUNCTION investment_cap_update() RETURNS TRIGGER AS $investment_cap$
	BEGIN
		-- not enough capital
		IF ((SELECT capital FROM portfolio WHERE pfid=NEW.pfid) < 
		((SELECT sum(maxinvest) FROM pfsecurity WHERE pfid=NEW.pfid)-OLD.maxinvest+NEW.maxinvest)) THEN
			RAISE EXCEPTION 'Portfolio capital too small for this maximum investment.';
		END IF;
		RETURN NEW;
	END;
$investment_cap$ LANGUAGE plpgsql;

CREATE TRIGGER pfsecurity_update_maxinvest
BEFORE UPDATE ON pfsecurity
FOR EACH ROW
EXECUTE PROCEDURE investment_cap_update();

-- UPDATE capital
CREATE OR REPLACE FUNCTION check_capital() RETURNS TRIGGER AS $check_capital$
	BEGIN
		-- not enough capital
		IF (NEW.capital < (SELECT sum(maxinvest) FROM pfsecurity WHERE pfid=NEW.pfid)) THEN
			NEW.capital = OLD.capital;
			--RAISE EXCEPTION 'Portfolio capital too small for sum of maximum investments.';
		END IF;
		RETURN NEW;
	END;
$check_capital$ LANGUAGE plpgsql;

CREATE TRIGGER portfolio_update_capital
BEFORE UPDATE ON portfolio
FOR EACH ROW
EXECUTE PROCEDURE check_capital();

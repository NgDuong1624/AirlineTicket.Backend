-- =====================================================
-- CORE: Aircraft Model Seat Templates
-- Schema: flights
-- Depends on: 03_aircraft_models.sql
-- =====================================================

DO $$
DECLARE
    _modelB787 UUID := 'B7879000-BCDE-4F01-2345-6789ABCDEF01';
    _modelA350 UUID := 'A3509000-BCDE-4F01-2345-6789ABCDEF02';
    _modelA321 UUID := 'A3212000-BCDE-4F01-2345-6789ABCDEF03';
    _row INTEGER;
BEGIN

-- Helper table for columns
CREATE TEMPORARY TABLE IF NOT EXISTS temp_cols (col CHAR(1), col_index INTEGER);
TRUNCATE temp_cols;
INSERT INTO temp_cols VALUES ('A', 1), ('B', 2), ('C', 3), ('D', 4), ('E', 5), ('F', 6), ('G', 7), ('H', 8), ('K', 9);

-- -------------------------------------------------------
-- Boeing 787-9 Seat Template
-- -------------------------------------------------------

-- Business Class: Rows 1-5, 1-2-1 layout (A, D, G, K)
_row := 1;
WHILE _row <= 5 LOOP
    INSERT INTO flights.aircraft_model_seat_templates (id, aircraft_model_id, seat_number, seat_row, seat_column, seat_class, is_extra_legroom, price_multiplier)
    SELECT gen_random_uuid(), _modelB787, _row::VARCHAR(2) || col, _row::VARCHAR(2), col, 2, TRUE, 2.5
    FROM temp_cols 
    WHERE col IN ('A', 'D', 'G', 'K')
      AND NOT EXISTS (
          SELECT 1 FROM flights.aircraft_model_seat_templates 
          WHERE aircraft_model_id = _modelB787 AND seat_number = _row::VARCHAR(2) || col
      );
    _row := _row + 1;
END LOOP;

-- Premium Economy: Rows 10-15, 2-3-2 layout (A, C, D, F, G, H, K)
_row := 10;
WHILE _row <= 15 LOOP
    INSERT INTO flights.aircraft_model_seat_templates (id, aircraft_model_id, seat_number, seat_row, seat_column, seat_class, is_extra_legroom, price_multiplier)
    SELECT gen_random_uuid(), _modelB787, _row::VARCHAR(2) || col, _row::VARCHAR(2), col, 1, FALSE, 1.5
    FROM temp_cols 
    WHERE col IN ('A', 'C', 'D', 'F', 'G', 'H', 'K')
      AND NOT EXISTS (
          SELECT 1 FROM flights.aircraft_model_seat_templates 
          WHERE aircraft_model_id = _modelB787 AND seat_number = _row::VARCHAR(2) || col
      );
    _row := _row + 1;
END LOOP;

-- Economy: Rows 20-45, 3-3-3 layout (A, B, C, D, E, F, G, H, K)
_row := 20;
WHILE _row <= 45 LOOP
    INSERT INTO flights.aircraft_model_seat_templates (id, aircraft_model_id, seat_number, seat_row, seat_column, seat_class, is_extra_legroom, price_multiplier)
    SELECT gen_random_uuid(), _modelB787, _row::VARCHAR(2) || col, _row::VARCHAR(2), col, 0,
        CASE WHEN _row = 20 THEN TRUE ELSE FALSE END,
        CASE WHEN _row = 20 THEN 1.2 ELSE 1.0 END
    FROM temp_cols 
    WHERE col IN ('A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'K')
      AND NOT EXISTS (
          SELECT 1 FROM flights.aircraft_model_seat_templates 
          WHERE aircraft_model_id = _modelB787 AND seat_number = _row::VARCHAR(2) || col
      );
    _row := _row + 1;
END LOOP;

-- -------------------------------------------------------
-- Airbus A350-900 Seat Template
-- -------------------------------------------------------

-- Business Class: Rows 1-6, 1-2-1 layout (A, D, G, K)
_row := 1;
WHILE _row <= 6 LOOP
    INSERT INTO flights.aircraft_model_seat_templates (id, aircraft_model_id, seat_number, seat_row, seat_column, seat_class, is_extra_legroom, price_multiplier)
    SELECT gen_random_uuid(), _modelA350, _row::VARCHAR(2) || col, _row::VARCHAR(2), col, 2, TRUE, 2.5
    FROM temp_cols 
    WHERE col IN ('A', 'D', 'G', 'K')
      AND NOT EXISTS (
          SELECT 1 FROM flights.aircraft_model_seat_templates 
          WHERE aircraft_model_id = _modelA350 AND seat_number = _row::VARCHAR(2) || col
      );
    _row := _row + 1;
END LOOP;

-- Premium Economy: Rows 10-16, 2-4-2 layout (A, C, D, E, F, G, H, K)
_row := 10;
WHILE _row <= 16 LOOP
    INSERT INTO flights.aircraft_model_seat_templates (id, aircraft_model_id, seat_number, seat_row, seat_column, seat_class, is_extra_legroom, price_multiplier)
    SELECT gen_random_uuid(), _modelA350, _row::VARCHAR(2) || col, _row::VARCHAR(2), col, 1, FALSE, 1.5
    FROM temp_cols 
    WHERE col IN ('A', 'C', 'D', 'E', 'F', 'G', 'H', 'K')
      AND NOT EXISTS (
          SELECT 1 FROM flights.aircraft_model_seat_templates 
          WHERE aircraft_model_id = _modelA350 AND seat_number = _row::VARCHAR(2) || col
      );
    _row := _row + 1;
END LOOP;

-- Economy: Rows 20-46, 3-3-3 layout (A, B, C, D, E, F, G, H, K)
_row := 20;
WHILE _row <= 46 LOOP
    INSERT INTO flights.aircraft_model_seat_templates (id, aircraft_model_id, seat_number, seat_row, seat_column, seat_class, is_extra_legroom, price_multiplier)
    SELECT gen_random_uuid(), _modelA350, _row::VARCHAR(2) || col, _row::VARCHAR(2), col, 0,
        CASE WHEN _row = 20 THEN TRUE ELSE FALSE END,
        CASE WHEN _row = 20 THEN 1.2 ELSE 1.0 END
    FROM temp_cols 
    WHERE col IN ('A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'K')
      AND NOT EXISTS (
          SELECT 1 FROM flights.aircraft_model_seat_templates 
          WHERE aircraft_model_id = _modelA350 AND seat_number = _row::VARCHAR(2) || col
      );
    _row := _row + 1;
END LOOP;

-- -------------------------------------------------------
-- Airbus A321neo Seat Template
-- -------------------------------------------------------

-- Business Class: Rows 1-2, 2-2 layout (A, C, H, K)
_row := 1;
WHILE _row <= 2 LOOP
    INSERT INTO flights.aircraft_model_seat_templates (id, aircraft_model_id, seat_number, seat_row, seat_column, seat_class, is_extra_legroom, price_multiplier)
    SELECT gen_random_uuid(), _modelA321, _row::VARCHAR(2) || col, _row::VARCHAR(2), col, 2, TRUE, 2.0
    FROM temp_cols 
    WHERE col IN ('A', 'C', 'H', 'K')
      AND NOT EXISTS (
          SELECT 1 FROM flights.aircraft_model_seat_templates 
          WHERE aircraft_model_id = _modelA321 AND seat_number = _row::VARCHAR(2) || col
      );
    _row := _row + 1;
END LOOP;

-- Economy: Rows 3-38, 3-3 layout (A, B, C, G, H, K)
_row := 3;
WHILE _row <= 38 LOOP
    INSERT INTO flights.aircraft_model_seat_templates (id, aircraft_model_id, seat_number, seat_row, seat_column, seat_class, is_extra_legroom, price_multiplier)
    SELECT gen_random_uuid(), _modelA321, _row::VARCHAR(2) || col, _row::VARCHAR(2), col, 0,
        CASE WHEN _row IN (11, 12) THEN TRUE ELSE FALSE END,
        CASE WHEN _row IN (11, 12) THEN 1.2 ELSE 1.0 END
    FROM temp_cols 
    WHERE col IN ('A', 'B', 'C', 'G', 'H', 'K')
      AND NOT EXISTS (
          SELECT 1 FROM flights.aircraft_model_seat_templates 
          WHERE aircraft_model_id = _modelA321 AND seat_number = _row::VARCHAR(2) || col
      );
    _row := _row + 1;
END LOOP;

-- Cleanup
DROP TABLE IF EXISTS temp_cols;

END $$;

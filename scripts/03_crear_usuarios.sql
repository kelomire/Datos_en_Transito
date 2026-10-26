CREATE USER IF NOT EXISTS 'administrador'@'localhost'
IDENTIFIED BY 'Admin1234!';

CREATE USER IF NOT EXISTS 'desarrollo'@'localhost'
IDENTIFIED BY 'Desarrollo1234!';

GRANT ALL PRIVILEGES ON *.* 
TO 'administrador'@'localhost';

GRANT SELECT, INSERT, UPDATE, DELETE, EXECUTE
ON datos_transito.*
TO 'desarrollo'@'localhost';

FLUSH PRIVILEGES;

CREATE USER IF NOT EXISTS 'administrador'@'localhost' IDENTIFIED BY 'Admin1234!';
CREATE USER IF NOT EXISTS 'invitado'@'localhost' IDENTIFIED BY 'Invitado1234!';

GRANT ALL PRIVILEGES ON datos_transito.* TO 'administrador'@'localhost';
GRANT SELECT ON datos_transito.* TO 'invitado'@'localhost';

FLUSH PRIVILEGES;

CREATE TABLE users
(
    id serial PRIMARY KEY,
    email varchar(255) UNIQUE NOT NULL,
    password varchar(30) NOT NULL,
    name varchar(30) NOT NULL
);

CREATE TABLE refresh_tokens
(
    id uuid PRIMARY KEY,
    user_id integer NOT NULL REFERENCES users(id),
    device_uid uuid,
    expires timestamp NOT NULL
);

CREATE TABLE projects
(
    id serial PRIMARY KEY,
    name varchar(50) not null unique,
    user_id integer NOT NULL REFERENCES users(id)
);

CREATE TYPE task_status AS ENUM ('planned', 'ongoing', 'completed');

CREATE TABLE tasks
(
    id serial PRIMARY KEY,
    name varchar(50) not null,
    text varchar(5000) not null,
    user_id integer NOT NULL REFERENCES users(id),
    project_id integer NOT NULL REFERENCES projects(id),
    creation_date timestamp NOT NULL,
    status task_status not null default 'planned'
);

insert into users(email, password, name) values ('a@a.a', '123', 'Ivan'), ('b@b.b', '123', 'Sergey');
insert into projects(name, user_id) values ('test1', 1);
insert into tasks(name, text, user_id, project_id, creation_date, status) values
    ('test task', '', 1, 1, '01.01.2022', default),
    ('test task 2', '', 2, 1, '01.01.2022', 'planned'),
    ('ongoing task 1', '', 1, 1, '01.01.2022', 'ongoing'),
    ('ongoing task 2', '', 2, 1, '01.01.2022', 'ongoing'),
    ('completed task', '', 1, 1, '01.01.2022', 'completed');

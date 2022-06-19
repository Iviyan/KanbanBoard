/*
drop table users;
drop table refresh_tokens;
drop table projects;
drop table tasks;
drop table comments;
drop table project_members;
*/

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
    project_id integer NOT NULL REFERENCES projects(id) ON DELETE CASCADE,
    creation_date timestamp NOT NULL,
    status task_status not null default 'planned'
);

create index tasks_project_id_index on tasks(project_id);

CREATE TABLE comments
(
    id serial PRIMARY KEY,
    text varchar(5000) not null,
    user_id integer NOT NULL REFERENCES users(id),
    task_id integer NOT NULL REFERENCES tasks(id) ON DELETE CASCADE,
    creation_date timestamp NOT NULL
);

create index comments_task_id_index on comments(task_id);

create table project_members
(
    user_id integer NOT NULL REFERENCES users(id),
    project_id integer NOT NULL REFERENCES projects(id) ON DELETE CASCADE,
	primary key(project_id, user_id)
);

CREATE OR REPLACE FUNCTION projects_after_insert()
RETURNS trigger AS $$ BEGIN
         INSERT INTO project_members(user_id, project_id) VALUES(new.user_id, new.id);
    RETURN NEW;
END; $$ LANGUAGE 'plpgsql';

CREATE TRIGGER projects_after_insert
    AFTER INSERT ON projects
    FOR EACH ROW
    EXECUTE FUNCTION projects_after_insert();

/*
insert into users(email, password, name) values ('a@a.a', '123', 'Ivan'), ('b@b.b', '123', 'Sergey');
insert into projects(name, user_id) values ('test1', 1);
insert into tasks(name, text, user_id, project_id, creation_date, status) values
  ('test task', '', 1, 1, '01.01.2022', default),
  ('test task 2', '', 2, 1, '01.01.2022', 'planned'),
  ('ongoing task 1', '', 1, 1, '01.01.2022', 'ongoing'),
  ('ongoing task 2', '', 2, 1, '01.01.2022', 'ongoing'),
  ('completed task', '', 1, 1, '01.01.2022', 'completed');

select projects.id, projects.name, projects.user_id from project_members
inner join projects on projects.id = project_members.project_id
where project_members.user_id = 2;

select * from project_members where project_id = (select project_id from tasks where id = 1);
*/

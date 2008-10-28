use monodoc;
drop table person;
drop table status;
create table person (
	name varchar (80) not null,
	password varchar (20) not null,
	person_id int not null primary key auto_increment,
	last_serial int not null,
	last_update int not null,
	is_admin int not null
);

create table status (
	person_id int not null,
	serial int not null,
	status int not null
);

insert into person (name, password, last_serial, is_admin) values ('miguel@ximian.com', 'login1', 0, 1);
insert into person (name, password, last_serial) values ('nat@nat.org', 'login2', 0);


USE webcompare;

DROP TABLE IF EXISTS `messages`;
DROP TABLE IF EXISTS `nodes`;
DROP TABLE IF EXISTS `master`;
DROP TABLE IF EXISTS `filters`;
CREATE TABLE  `filters` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `is_rx` bit(1) NOT NULL,
  `name_filter` varchar(100) DEFAULT NULL,
  `typename_filter` varchar(100) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=MyISAM AUTO_INCREMENT=9 DEFAULT CHARSET=latin1;

CREATE TABLE  `master` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `reference` varchar(100) CHARACTER SET utf8 NOT NULL,
  `profile` varchar(100) CHARACTER SET utf8 NOT NULL,
  `assembly` varchar(100) CHARACTER SET utf8 NOT NULL,
  `detail_level` varchar(25) COLLATE utf8_bin NOT NULL,
  `last_updated` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `active` bit(1) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `master_idx_1` (`reference`,`profile`,`assembly`,`detail_level`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=522 DEFAULT CHARSET=utf8 COLLATE=utf8_bin;

CREATE TABLE  `messages` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `node_name` varchar(200) COLLATE utf8_bin NOT NULL,
  `master_id` int(10) unsigned NOT NULL,
  `is_todo` bit(1) NOT NULL,
  `message` varchar(256) COLLATE utf8_bin DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `master_id_msg_idx` (`master_id`),
  KEY `name_nodename_idx` (`node_name`,`master_id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=139995 DEFAULT CHARSET=utf8 COLLATE=utf8_bin ROW_FORMAT=DYNAMIC;

CREATE TABLE  `nodes` (
  `node_name` varchar(256) CHARACTER SET utf8 COLLATE utf8_bin NOT NULL,
  `master_id` int(10) unsigned NOT NULL,
  `child_id` int(11) NOT NULL,
  `parent_name` varchar(256) CHARACTER SET utf8 COLLATE utf8_bin NOT NULL,
  `comparison_type` int(11) NOT NULL,
  `status` int(11) NOT NULL,
  `extras` int(11) NOT NULL,
  `missing` int(11) NOT NULL,
  `present` int(11) NOT NULL,
  `warning` int(11) NOT NULL,
  `todo` int(11) NOT NULL,
  `niex` int(11) NOT NULL,
  `throwsnie` tinyint(1) NOT NULL,
  `has_children` tinyint(1) NOT NULL,
  `has_messages` tinyint(1) NOT NULL,
  `name` varchar(128) CHARACTER SET utf8 COLLATE utf8_bin DEFAULT NULL,
  `typename` varchar(128) CHARACTER SET utf8 COLLATE utf8_bin DEFAULT NULL,
  PRIMARY KEY (`node_name`,`master_id`),
  KEY `master_id_nodes_idx` (`master_id`),
  KEY `master_parent_idx` (`parent_name`,`master_id`) USING BTREE
) ENGINE=MyISAM DEFAULT CHARSET=ascii COLLATE=ascii_bin;

DROP PROCEDURE IF EXISTS `get_children`;
DROP PROCEDURE IF EXISTS `get_master_id`;
DROP PROCEDURE IF EXISTS `get_messages`;
DROP PROCEDURE IF EXISTS `get_node_by_name`;
DROP PROCEDURE IF EXISTS `insert_master`;
DROP PROCEDURE IF EXISTS `update_active_master`;
DELIMITER $$

CREATE PROCEDURE  `get_children`(IN master_id INT, IN parent_name VARCHAR(128))
BEGIN
SELECT * FROM nodes n WHERE n.master_id = master_id AND n.parent_name = parent_name ORDER BY child_id;
END
$$

CREATE PROCEDURE  `get_master_id`(in reference varchar(100), in profile varchar (100), in assembly varchar(100), in detail_level varchar (25))
BEGIN
SELECT m.id, m.last_updated FROM master m
WHERE m.reference = reference AND m.profile = profile AND m.assembly = assembly
ORDER BY last_updated DESC
LIMIT 1;
END
$$

CREATE PROCEDURE  `get_messages`(IN master_id INT, IN nodename VARCHAR(128))
BEGIN
SELECT m.is_todo, m.message
FROM messages m
WHERE m.master_id = master_id AND node_name = nodename;
END
$$

CREATE PROCEDURE  `get_node_by_name`(in master_id int, in nodename varchar(128))
BEGIN
SELECT n.* FROM nodes n
WHERE n.master_id = master_id AND n.node_name = nodename;
END
$$

CREATE PROCEDURE  `insert_master`( IN reference varchar(100), IN profile varchar (100), IN assembly varchar (100), IN last_updated timestamp, OUT id int)
BEGIN
INSERT INTO master (reference, profile, assembly, last_updated, active) VALUES (reference, profile, assembly, last_updated, FALSE);
SET id = LAST_INSERT_ID();
END
$$

CREATE PROCEDURE  `update_active_master`(IN master_id INT)
BEGIN
DECLARE reference varchar(128);
DECLARE profile varchar(128);
DECLARE assembly varchar(128);

SELECT m.reference, m.profile, m.assembly
FROM master m
WHERE m.id = master_id
INTO @reference, @profile, @assembly;

UPDATE master m
SET m.active = TRUE
WHERE m.id = master_id;

UPDATE master m
SET m.active = FALSE
WHERE m.id <> master_id AND m.reference = @reference AND m.profile = @profile AND m.assembly = @assembly;
END
$$



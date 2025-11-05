测试接口：
列表：GET /api/company/list?page=1&pageSize=10
单条：GET /api/company/get/1
新增：POST /api/company/add （JSON body）
更新：PUT /api/company/update/1 （JSON body）
删除：DELETE /api/company/delete/1

重点★★★★★

新增/更新
{
  "AboutName": "企业文化",
  "ViewFlag": true,
  "Content": "公司文化注重创新与团队合作。",
  "ClickNumber": 0,
  "Sequence": 1,
  "GroupID": "A01",
  "Exclusive": "内部资料",
  "NavType": 2,
  "ChildFlag": false,
  "AddTime": "2025-10-29T10:00:00",
  "UpdateTime": "2025-10-29T10:00:00"
}

测试环境：
操作系统：windows2022数据中心版 64位
开发语言： .net core9
编译工具：vscode
AI辅助工具：chatGPT
数据库版本：
1、SQL2022-SSEI-Expr.exe
2、mysql-8.4.7-winx64.msi
3、postgresql-18.0-2-windows-x64-binaries.zip
4、mongodb-windows-x86_64-8.0.15-signed.msi
5、OracleXE213_Win64.zip
6、mariadb-11.8.3-winx64.msi
7、Redis-8.2.3-Windows-x64-msys2-with-Service.zip
8、PolarDB_15.14.5.0-b4011ae4-ubuntu22.04_amd64.deb （ubuntu22.04虚拟机）
工具软件：
1、SSMS-Setup-CHS.exe
2、Navicat Premium 17
3、mongosh-2.5.9-x64.msi
4、xshell7mfb.rar
5、Xftp7pj.rar


---------------------------------------------------------------------------------------------------------------------------------------
9:34 2025/11/5

在ubuntu安装二进行安装包，需要下载安装xftp用来上传文件

安装后查安装的目录
dpkg -L polardb-for-postgresql | grep bin
或者
which psql

创建数据库目录
sudo mkdir -p /u01/polardb_pg/data
sudo chown $USER:$USER /u01/polardb_pg/data


修改配置文件，外网访问
nano /u01/polardb_pg/data/postgresql.conf


port:43678

配置成 系统服务
sudo nano /etc/systemd/system/polardb.service
----------------------------------------------------------------

[Unit]
Description=PolarDB for PostgreSQL
After=network.target

[Service]
Type=forking
User=zhq145
Group=zhq145
ExecStart=/u01/polardb_pg/bin/pg_ctl -D /u01/polardb_pg/data -l /u01/polardb_pg/data/logfile start
ExecStop=/u01/polardb_pg/bin/pg_ctl -D /u01/polardb_pg/data stop
ExecReload=/u01/polardb_pg/bin/pg_ctl -D /u01/polardb_pg/data reload
Restart=always

[Install]
WantedBy=multi-user.target
----------------------------------------------------------------

上传 polardb.service 到你有权限的目录，例如 ~/Downloads
用 sudo 移动到 /etc/systemd/system/：
sudo mv ~/Downloads/polardb.service /etc/systemd/system/
sudo chmod 644 /etc/systemd/system/polardb.service

# 重新加载 systemd 配置
sudo systemctl daemon-reload

# 启用开机自启
sudo systemctl enable polardb

# 启动服务
sudo systemctl start polardb

sudo systemctl stop polardb

sudo systemctl restart polardb

# 查看状态
sudo systemctl status polardb


停polardb服务后，我们就可以安全地进入单用户模式创建超级用户 postgres：
/u01/polardb_pg/bin/postgres --single -D /u01/polardb_pg/data postgres


CREATE ROLE postgres WITH LOGIN SUPERUSER PASSWORD '123456';

两次CTRL+D退出

查是否侦听端口
ss -tunlp | grep 43678

防火墙放行端口
sudo ufw allow 43678/tcp
sudo ufw reload


验证登录（本机）
psql -U postgres -h localhost -p 43678


超级管理员改普通用户密码
ALTER USER appuser WITH PASSWORD '123456';

创建数据库(注意：创建数据库或者用户时，系统有时全变成小写)
CREATE DATABASE MyAppDB
    WITH
    ENCODING = 'UTF8'
    LC_COLLATE = 'zh_CN.UTF-8'
    LC_CTYPE = 'zh_CN.UTF-8'
    TEMPLATE = template0;

创建用户
CREATE USER AppUser WITH 
    PASSWORD 'StrongPassword123'
    LOGIN;

授权用户管理数据库
-- 授权全部对象
GRANT CONNECT ON DATABASE myappdb TO appuser;

\c MyAppDB   -- 切换到 MyAppDB

-- 给 AppUser 管理 MyAppDB 内的对象权限
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO appuser;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO appuser;
GRANT ALL PRIVILEGES ON SCHEMA public TO appuser;

列数据库
\l
列数据表
\dt







创建表
CREATE TABLE "ZGBao_About" (
    "ID" SERIAL PRIMARY KEY,
    "AboutName" TEXT,
    "ViewFlag" BOOLEAN,
    "Content" TEXT,
    "ClickNumber" INTEGER,
    "Sequence" INTEGER,
    "GroupID" TEXT,
    "Exclusive" TEXT,
    "NavType" INTEGER,
    "ChildFlag" BOOLEAN,
    "AddTime" TIMESTAMP,
    "UpdateTime" TIMESTAMP
);


INSERT INTO "ZGBao_About" (
    "AboutName", "ViewFlag", "Content", "ClickNumber", "Sequence",
    "GroupID", "Exclusive", "NavType", "ChildFlag", "AddTime", "UpdateTime"
) VALUES
('关于我们1', TRUE, '这是内容1', 10, 1, 'G1', 'E1', 1, FALSE, NOW(), NOW()),
('关于我们2', FALSE, '这是内容2', 15, 2, 'G1', 'E1', 1, TRUE, NOW(), NOW()),
('关于我们3', TRUE, '这是内容3', 20, 3, 'G2', 'E2', 2, FALSE, NOW(), NOW()),
('关于我们4', TRUE, '这是内容4', 5, 4, 'G2', 'E2', 2, TRUE, NOW(), NOW()),
('关于我们5', FALSE, '这是内容5', 30, 5, 'G3', 'E3', 1, FALSE, NOW(), NOW()),
('关于我们6', TRUE, '这是内容6', 25, 6, 'G3', 'E3', 1, TRUE, NOW(), NOW()),
('关于我们7', FALSE, '这是内容7', 12, 7, 'G4', 'E4', 2, FALSE, NOW(), NOW()),
('关于我们8', TRUE, '这是内容8', 8, 8, 'G4', 'E4', 2, TRUE, NOW(), NOW()),
('关于我们9', FALSE, '这是内容9', 18, 9, 'G5', 'E5', 1, FALSE, NOW(), NOW()),
('关于我们10', TRUE, '这是内容10', 22, 10, 'G5', 'E5', 1, TRUE, NOW(), NOW());


重点★★★★★编译过程失败，编译成功后，缺失文件没有 polar_feature_utils 扩展

ubuntu22.04 安装 从源码编译安装 PolarDB for PostgreSQL 

编译后的目录，或许稍有不同
ls /usr/local/pgsql/bin

创建数据库目录及授权
sudo mkdir -p /usr/local/polardb/data
sudo chown $USER:$USER /usr/local/polardb/data

初始化数据库目录
/usr/local/pgsql/bin/initdb -D /usr/local/polardb/data

# 查看当前 remote
git remote -v

# 将 origin 指向阿里云
git remote set-url origin https://codeup.aliyun.com/apsaradb/PolarDB-for-PostgreSQL.git

# 验证是否修改成功
git remote -v

15:56 2025/11/4

现在的实现已经满足以下特点：
多数据库支持
SQL Server、MySQL、MariaDB、PostgreSQL、Oracle、MongoDB 都可以使用相同的接口进行增删改查（RESTful 风格）。
Redis 缓存可选
Redis 仅作为缓存层使用。
CachedDatabaseService 已经加了异常处理，当 Redis 不可用或暂停时，接口会直接访问数据库。
这意味着 Redis 是否启用，对接口功能没有影响。
接口功能完整性
列表分页：GET /api/company/list?page=1&pageSize=10
单条读取：GET /api/company/get/1
新增：POST /api/company/add
更新：PUT /api/company/update/1
删除：DELETE /api/company/delete/1
缓存策略
默认 TTL 5 分钟（可在 appsettings.json 配置）
分页列表和单条记录都有缓存
数据更新/删除会清理相关缓存


10:32 2025/11/4

配置示例文件：redis..conf

############################
# 绑定监听地址
############################
# 监听所有 IPv4 和 IPv6 地址
bind 0.0.0.0 ::

# 开启保护模式（防止未授权访问）
protected-mode yes

############################
# 密码和 ACL 用户
############################

# 老式全局密码（可留空，如果只用 ACL 可注释掉）
# requirepass YourGlobalPasswordHere

# ACL 用户示例
# 管理员用户：可以执行所有命令、访问所有 key
user admin on >AdminStrongPassword ~* +@all

# 只读用户：只能读取 key，不能写
# 只允许访问 key 前缀为 cache: 的数据
user readonly on >ReadonlyStrongPassword ~cache:* +@read

############################
# 日志与持久化
############################
# 日志文件
logfile "C:/Redis/logs/redis.log"

# RDB 持久化（每 900 秒至少有 1 个 key 改动时保存）
save 900 1
# AOF 持久化（可选）
appendonly yes
appendfilename "appendonly.aof"




Redis 启动后，ACL保护模式检测到 默认用户有认证，就允许外网访问。
user default on >DefaultStrongPassword ~* +@all
或者禁用黙认用户
user default off


通过Navicat Premium 17连接要加PING权限
user AppUser on >123456 ~* +@read +@write +PING


接口权限用户授权
user AppUser on >123456 ~* +@read +@write +ping +echo +client +info


15:54 2025/11/3

dotnet add package Oracle.ManagedDataAccess.Core

14:16 2025/11/3

sqlplus / as sysdba

如果执行上面的命令报错，说明没有指定黙认数据库
ERROR:
ORA-12560: TNS: 协议适配器错误

解决办法：
set ORACLE_SID=XE
ORACLE_SID 告诉 SQL*Plus 你要连接哪个数据库实例（在 Oracle XE 中默认就是 XE）。


查看当前数据库中的所有用户
SELECT username FROM all_users ORDER BY username;

查看当前登录的用户
SHOW USER;

-- 查看当前容器
SHOW CON_NAME;

-- 切换到 PDB
ALTER SESSION SET CONTAINER = XEPDB1;
SHOW CON_NAME;

-- 创建普通管理员用户
CREATE USER admin IDENTIFIED BY "123456";
GRANT CREATE SESSION, DBA TO admin;

-- 测试登录
-- sqlplus admin/123456@localhost:1521/XEPDB1

DBA 是 Oracle 的标准管理员角色，拥有几乎所有操作权限，包括创建表、创建用户、管理表空间、启动/关闭数据库等。

Navicat Premium 17远程连接
将 Service Name 从 ORCL 改为 XEPDB1

在 Oracle XE 21c 里，可以通过 在 PDB 下创建用户 + 授权到专用 Schema 来实现类似 MySQL 的“单库访问”隔离。


CREATE USER AppUser IDENTIFIED BY "123456";
GRANT CREATE SESSION TO AppUser;
GRANT CREATE TABLE, CREATE VIEW, CREATE SEQUENCE TO AppUser;

授于触发器权限
GRANT CREATE TRIGGER TO APPUSER;


-- 给 APPUSER 在 USERS 表空间分配 100M 配额
ALTER USER AppUser QUOTA 100M ON USERS;

-- 或者无限配额
ALTER USER AppUser QUOTA UNLIMITED ON USERS;




sqlplus AppUser/123456@localhost:1521/XEPDB1

重点★★★★★  
重启 Oracle 后记录消失，没有执行 COMMIT，这些变更是 未提交事务。



INSERT INTO ZGBAO_ABOUT (AboutName, ViewFlag, Content, ClickNumber, Sequence, GroupID, "Exclusive", NavType, ChildFlag, AddTime, UpdateTime)
VALUES ('关于我们1','Y','内容示例1',10,1,'G1','N',1,'N',SYSDATE,SYSDATE);
COMMIT;


SELECT COUNT(*) AS RECORD_COUNT FROM ZGBAO_ABOUT;


SELECT ID, AboutName FROM ZGBAO_ABOUT;

Oracle新建表的顺序

第一步：删除旧表和旧序列（如果存在）
BEGIN
  EXECUTE IMMEDIATE 'DROP TABLE ZGBAO_ABOUT';
EXCEPTION
  WHEN OTHERS THEN
    IF SQLCODE != -942 THEN  -- ORA-00942: 表或视图不存在
      RAISE;
    END IF;
END;
/

BEGIN
  EXECUTE IMMEDIATE 'DROP SEQUENCE ZGBAO_ABOUT_SEQ';
EXCEPTION
  WHEN OTHERS THEN
    IF SQLCODE != -2289 THEN  -- ORA-02289: 序列不存在
      RAISE;
    END IF;
END;
/

第二步：重新创建表

CREATE TABLE ZGBAO_ABOUT (
    ID NUMBER PRIMARY KEY,
    AboutName VARCHAR2(200),
    ViewFlag CHAR(1),
    Content CLOB,
    ClickNumber NUMBER,
    Sequence NUMBER,
    GroupID VARCHAR2(50),
    "Exclusive" CHAR(1),
    NavType NUMBER,
    ChildFlag CHAR(1),
    AddTime DATE,
    UpdateTime DATE
);


第三步：创建自增序列

CREATE SEQUENCE ZGBAO_ABOUT_SEQ
  START WITH 1
  INCREMENT BY 1
  NOCACHE
  NOCYCLE;

第四步（可选）：创建触发器让 ID 自动递增
如果你希望插入时不用写 ID（比如从你的 .NET 程序中直接插入），建议加上这个触发器

CREATE OR REPLACE TRIGGER ZGBAO_ABOUT_TRG
BEFORE INSERT ON ZGBAO_ABOUT
FOR EACH ROW
BEGIN
  IF :NEW.ID IS NULL THEN
    SELECT ZGBAO_ABOUT_SEQ.NEXTVAL INTO :NEW.ID FROM DUAL;
  END IF;
END;
/


第五步：插入测试数据

INSERT INTO ZGBAO_ABOUT (
  AboutName, ViewFlag, Content, ClickNumber, Sequence, GroupID, "Exclusive", NavType, ChildFlag, AddTime, UpdateTime
) VALUES (
  '关于我们1', 'Y', '内容示例1', 10, 1, 'G1', 'N', 1, 'N', SYSDATE, SYSDATE
);

COMMIT;


第六步：验证数据

SELECT ID, AboutName FROM ZGBAO_ABOUT;


11:37 2025/11/3

mariadb数据库安装时设置黙认端口3307（防止与mysql冲突）

重点★★★★★

一、创建数据库
CREATE DATABASE MyAppDB CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;
二、创建专属用户
CREATE USER 'AppUser'@'%' IDENTIFIED BY '123456';
三、赋予专属权限
GRANT ALL PRIVILEGES ON MyAppDB.* TO 'AppUser'@'%';
四、刷新权限
FLUSH PRIVILEGES;


9:39 2025/11/3

由于mongodb没有自增型ID，它是_id（唯一字符串），自已新增一个自增型ID值，这样与业务逻辑统一


var counter = 1;
db.ZGBao_About.find().sort({ _id: 1 }).forEach(function(doc) {
  db.ZGBao_About.updateOne(
    { _id: doc._id },
    { $set: { ID: counter } }
  );
  counter++;
});




14:36 2025/11/1

安装Mongo依赖库安装包
dotnet add package MongoDB.Driver




修改配置文件 mongod.cfg
# 数据存储路径
storage:
  dbPath: "D:\mongodb\data"

# 日志文件路径
systemLog:
  destination: file
  path: "D:\mongodb\log\mongod.log"
  logAppend: true

# 网络端口
net:
  bindIp: 0.0.0.0
  port: 27017

# 启用身份验证
security:
  authorization: "enabled"


启用了身份认证，任何连接都必须使用有权限的用户。所以第一步是创建 超级管理员用户

使用 mongosh 连接
mongosh

切换到 admin 数据库  (admin 数据库是 MongoDB 系统管理数据库，管理员用户都创建在这里。)
use admin

创建超级管理员用户

db.createUser({
  user: "admin",
  pwd: "123456",
  roles: [ { role: "root", db: "admin" } ]
})


role: "root" 是 最高权限，可以：
管理所有数据库
创建/删除用户
修改配置
管理权限


用管理员账号重新登录（测试认证是否生效)
mongosh -u admin -p 123456 --authenticationDatabase admin

创建隔离用户及数据库
use MyAppDB

创建隔离用户 AppUser
db.createUser({
  user: "AppUser",
  pwd: "123456", 
  roles: [ { role: "readWrite", db: "MyAppDB" } ]
})


连接时指定默认数据库
mongosh "mongodb://AppUser:123456@127.0.0.1:27017/MyAppDB"


创建集合，并加上约束
use MyAppDB

db.createCollection("ZGBao_About", {
  validator: {
    $jsonSchema: {
      bsonType: "object",
      required: ["AboutName", "ViewFlag", "ClickNumber", "Sequence", "GroupID", "Exclusive", "NayType", "ChildFlag", "AddTime", "UpdateTime"],
      properties: {
        AboutName: { bsonType: "string", description: "必须是字符串" },
        ViewFlag: { bsonType: "bool", description: "必须是布尔值" },
        Content: { bsonType: "string", description: "备注，可选" },
        ClickNumber: { bsonType: "int", description: "数字" },
        Sequence: { bsonType: "int", description: "数字" },
        GroupID: { bsonType: "string", description: "文本" },
        Exclusive: { bsonType: "string", description: "文本" },
        NayType: { bsonType: "int", description: "数字" },
        ChildFlag: { bsonType: "bool", description: "布尔值" },
        AddTime: { bsonType: "date", description: "日期/时间" },
        UpdateTime: { bsonType: "date", description: "日期/时间" }
      }
    }
  },
  validationLevel: "strict",
  validationAction: "error"
})




12:02 2025/11/1


调试业务逻辑之前安装PostgreSQL依赖库安装包
dotnet add package Npgsql

PostgreSQL创建数据库
CREATE DATABASE "MyAppDB"
WITH 
OWNER = postgres
ENCODING = 'UTF8'
TEMPLATE = template0;

创建专属用户
CREATE USER "AppUser" WITH PASSWORD '123456';

切换到新数据库
\c "MyAppDB"

授权用户访问数据库（读写）
-- 允许 AppUser 连接数据库
GRANT CONNECT ON DATABASE "MyAppDB" TO "AppUser";

-- 授予 AppUser 在 public schema 的权限
GRANT USAGE, CREATE ON SCHEMA public TO "AppUser";

-- 授予读写权限（表和序列）
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO "AppUser";
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO "AppUser";



创建表
CREATE TABLE "ZGBao_About" (
    "ID" SERIAL PRIMARY KEY,
    "AboutName" TEXT,
    "ViewFlag" BOOLEAN,
    "Content" TEXT,
    "ClickNumber" INTEGER,
    "Sequence" INTEGER,
    "GroupID" TEXT,
    "Exclusive" TEXT,
    "NavType" INTEGER,
    "ChildFlag" BOOLEAN,
    "AddTime" TIMESTAMP,
    "UpdateTime" TIMESTAMP
);


INSERT INTO "ZGBao_About" (
    "AboutName", "ViewFlag", "Content", "ClickNumber", "Sequence",
    "GroupID", "Exclusive", "NavType", "ChildFlag", "AddTime", "UpdateTime"
) VALUES
('关于我们1', TRUE, '这是内容1', 10, 1, 'G1', 'E1', 1, FALSE, NOW(), NOW()),
('关于我们2', FALSE, '这是内容2', 15, 2, 'G1', 'E1', 1, TRUE, NOW(), NOW()),
('关于我们3', TRUE, '这是内容3', 20, 3, 'G2', 'E2', 2, FALSE, NOW(), NOW()),
('关于我们4', TRUE, '这是内容4', 5, 4, 'G2', 'E2', 2, TRUE, NOW(), NOW()),
('关于我们5', FALSE, '这是内容5', 30, 5, 'G3', 'E3', 1, FALSE, NOW(), NOW()),
('关于我们6', TRUE, '这是内容6', 25, 6, 'G3', 'E3', 1, TRUE, NOW(), NOW()),
('关于我们7', FALSE, '这是内容7', 12, 7, 'G4', 'E4', 2, FALSE, NOW(), NOW()),
('关于我们8', TRUE, '这是内容8', 8, 8, 'G4', 'E4', 2, TRUE, NOW(), NOW()),
('关于我们9', FALSE, '这是内容9', 18, 9, 'G5', 'E5', 1, FALSE, NOW(), NOW()),
('关于我们10', TRUE, '这是内容10', 22, 10, 'G5', 'E5', 1, TRUE, NOW(), NOW());



10:12 2025/11/1


从 PostgreSQL 15 开始，如果你在初始化时启用了 scram-sha-256 或 md5 密码认证方式（通过 --auth-local / --auth-host），必须在 initdb 时指定超级用户密码
重点★★★★★ windows系统，数据目录必须在C盘，权限问题，目录是通过命令自已创建
initdb -D "C:\pgdata" -U postgres -W --auth-local=scram-sha-256 --auth-host=scram-sha-256

启动数据库
pg_ctl -D "C:\pgdata" -l "C:\pgdata\logfile" start

测试连接
psql -U postgres -h localhost -p 5432

注册为 Windows 服务（开机自启）
pg_ctl register -N "PostgreSQL18" -D "C:\pgdata" -S auto

开机自动启动数据库
net start "PostgreSQL18"

遇到服务无法启动时，先排查

tasklist | findstr postgres

停止所有 PostgreSQL 进程
pg_ctl -D "C:\pgdata" stop


修改 postgresql.conf（允许所有地址监听）
listen_addresses = '*'    # 允许所有网卡（改动后需要重启）
port = 5432               # 默认端口（若保持默认可不改）

修改 pg_hba.conf（允许任意 IP 访问并用 SCRAM 认证）
# 允许任何外网/局域网 IP 用 scram-sha-256 登录（仅测试用）
host    all     all     0.0.0.0/0       scram-sha-256
host    all     all     ::0/0           scram-sha-256


14:45 2025/10/31

CREATE TABLE `ZGBao_About` (
    `ID` INT NOT NULL AUTO_INCREMENT COMMENT '自动编号',
    `AboutName` VARCHAR(255) NOT NULL COMMENT '文本',
    `ViewFlag` BOOLEAN DEFAULT FALSE COMMENT '是/否',
    `Content` TEXT COMMENT '备注',
    `ClickNumber` INT DEFAULT 0 COMMENT '数字',
    `Sequence` INT DEFAULT 0 COMMENT '数字',
    `GroupID` VARCHAR(100) COMMENT '文本',
    `Exclusive` VARCHAR(100) COMMENT '文本',
    `NavType` INT DEFAULT 0 COMMENT '数字',
    `ChildFlag` BOOLEAN DEFAULT FALSE COMMENT '是/否',
    `AddTime` DATETIME DEFAULT CURRENT_TIMESTAMP COMMENT '日期/时间',
    `UpdateTime` DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT '日期/时间',
    PRIMARY KEY (`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='ZGBao_About 表';

问题：表名在GUI下执行直接变小写，要改my.ini(不推荐)
修改 MySQL 配置 lower_case_table_names=0

13:29 2025/10/31

MySQL 配置（my.ini）开启外网访问
[mysqld]
# 允许所有 IP 访问
bind-address = 0.0.0.0


保证 root 可以远程访问：

-- 给 root 创建远程访问权限（所有主机）
CREATE USER 'root'@'%' IDENTIFIED BY '123456';
GRANT ALL PRIVILEGES ON *.* TO 'root'@'%' WITH GRANT OPTION;
FLUSH PRIVILEGES;

-- 验证用户
SELECT user, host FROM mysql.user;

结果应包含：

| root | localhost |
| root | %         |

% 表示允许任何 IP 访问。

防火墙开放3306端口

重点★★★★★

一、创建数据库
CREATE DATABASE myappdb CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;
二、创建专属用户
CREATE USER 'appuser'@'%' IDENTIFIED BY '123456';
三、赋予专属权限
GRANT ALL PRIVILEGES ON myappdb.* TO 'appuser'@'%';
四、刷新权限
FLUSH PRIVILEGES;




8:52 2025/10/30


重点★★★★★ 在调试过程遇到的问题
用Postman测试应用
http://localhost:5005/api/company/list?page=1&pageSize=10
返回报错
{
    "success": false,
    "code": 5000,
    "message": "已成功与服务器建立连接，但是在登录过程中发生错误。 (provider: SSL 提供程序, error: 0 - 证书链是由不受信任的颁发机构颁发的。)",
    "timestamp": 1761804534799
}

原因分析

SQL Server 的加密通道启用了 加密（Encrypt=True），但客户端（你的 .NET 9 应用）不信任 SQL Server 的 SSL 证书。
通常出现在：
你的 SQL Server 使用了自签名证书（默认安装就是这样）；
或客户端要求验证服务器证书；
或客户端和 SQL Server 在不同主机（例如你这台是本机，服务器在 192.168.10.248）。

解决方法
保留加密但信任服务器证书，最简单的方式（尤其在内网）：在连接字符串中设置：
"connectionString": "Server=192.168.10.248;Database=MyAppDB;User Id=AppUser;Password=123456;TrustServerCertificate=True;"




建表
CREATE TABLE [dbo].[ZGBao_About] (
    [ID] INT IDENTITY(1,1) PRIMARY KEY,            -- Access 自动编号 → SQL Server IDENTITY
    [AboutName] NVARCHAR(255) NOT NULL,            -- 文本 → NVARCHAR，可根据实际长度调整
    [ViewFlag] BIT DEFAULT 0,                       -- 是/否 → BIT
    [Content] NVARCHAR(MAX) NULL,                  -- 备注 → NVARCHAR(MAX)
    [ClickNumber] INT DEFAULT 0,                    -- 数字 → INT
    [Sequence] INT DEFAULT 0,                       -- 数字 → INT
    [GroupID] NVARCHAR(50) NULL,                   -- 文本 → NVARCHAR(50)
    [Exclusive] NVARCHAR(50) NULL,                 -- 文本 → NVARCHAR(50)
    [NavType] INT DEFAULT 0,                        -- 数字 → INT
    [ChildFlag] BIT DEFAULT 0,                      -- 是/否 → BIT
    [AddTime] DATETIME DEFAULT GETDATE(),          -- 日期/时间 → DATETIME，默认当前时间
    [UpdateTime] DATETIME NULL                      -- 日期/时间 → DATETIME，可为空
);

INSERT INTO [dbo].[ZGBao_About] 
([AboutName], [ViewFlag], [Content], [ClickNumber], [Sequence], [GroupID], [Exclusive], [NavType], [ChildFlag], [AddTime], [UpdateTime])
VALUES
('公司简介', 1, N'我们是一家专注于技术创新的公司，致力于提供优质的软件服务。', 128, 1, N'1', N'公共', 1, 0, GETDATE(), NULL),

('企业文化', 1, N'以客户为中心、以创新为动力、以质量求生存。', 96, 2, N'1', N'公共', 1, 0, GETDATE(), NULL),

('联系我们', 1, N'联系电话：400-800-1234，邮箱：info@myapp.com。', 43, 3, N'1', N'公共', 2, 0, GETDATE(), NULL),

('公司地址', 1, N'总部位于北京市朝阳区创新科技园A座。', 52, 4, N'1', N'公共', 2, 0, GETDATE(), NULL),

('发展历程', 0, N'公司成立于2010年，经过多年发展，现已成为行业领先企业。', 71, 5, N'1', N'公共', 1, 0, GETDATE(), NULL),

('招聘信息', 1, N'诚聘前端工程师、后端工程师、测试工程师。', 29, 6, N'2', N'招聘', 1, 1, GETDATE(), NULL),

('服务范围', 1, N'提供网站开发、APP开发、系统集成、运维等全方位服务。', 87, 7, N'2', N'服务', 1, 0, GETDATE(), NULL),

('荣誉资质', 1, N'获得ISO9001质量管理体系认证，多项行业奖项。', 63, 8, N'3', N'展示', 1, 0, GETDATE(), NULL),

('合作伙伴', 1, N'与多家知名互联网企业建立了长期战略合作关系。', 45, 9, N'3', N'合作', 2, 1, GETDATE(), NULL),

('网站声明', 0, N'本网站所有内容版权归本公司所有，未经许可不得转载。', 12, 10, N'4', N'政策', 2, 0, GETDATE(), NULL);


配置文件
{
  "CurrentDb": "SqlServer",
  "databases": {
    "SqlServer": {
      "type": "SqlServer",
      "connectionString": "Server=.;Database=MyDb;User Id=sa;Password=123;"
    }
  },
  "modules": {
    "company": {
      "table": "CompanyInfo",
      "allowedFields": ["Name","Address","Tel","Email","About","Logo"],
      "requiredFields": ["Name","About"]
    },
    "newslist": {
      "table": "NewsList",
      "allowedFields": ["CategoryName","Description"],
      "requiredFields": ["CategoryName"]
    },
    "news": {
      "table": "News",
      "allowedFields": ["Title","Content","CategoryId","Author","Status"],
      "requiredFields": ["Title","Content"]
    },
    "productlist": {
      "table": "ProductList",
      "allowedFields": ["CategoryName","Description"],
      "requiredFields": ["CategoryName"]
    },
    "product": {
      "table": "Product",
      "allowedFields": ["Name","Description","Price","CategoryId","Status"],
      "requiredFields": ["Name","Price"]
    },
    "slide": {
      "table": "Slide",
      "allowedFields": ["Title","ImageUrl","LinkUrl","Sequence","IsActive"],
      "requiredFields": ["Title","ImageUrl"]
    },
    "siteinfo": {
      "table": "SiteConfig",
      "allowedFields": ["SiteName","Keywords","Description","Copyright"],
      "requiredFields": ["SiteName"]
    }
  }
}

为保持简洁一致：
模块名 {module} 建议用小写英文字母；
表名 table 对应数据库实际表名；
保持一对一映射（模块名 ≈ 表逻辑名称）；
allowedFields、requiredFields 按数据库列名一致填写。


项目结构
DbRestfulApi/
├── appsettings.json
├── Program.cs
├── Controllers/
│   └── DynamicController.cs
├── Services/
│   ├── IDatabaseService.cs
│   ├── SqlServerService.cs
│   ├── MySqlService.cs
│   └── MongoService.cs
└── Models/
    └── ModuleConfig.cs




8:15 2025/9/29


标准返回风格
{
  "success": true,
  "code": 0,
  "message": "操作成功",
  "data": {
    "total": 57,
    "page": 1,
    "pageSize": 10,
    "items": [
      { "id": 3, "aboutName": "联系我们", "content": "这是第一条" },
      { "id": 4, "aboutName": "公司地址", "content": "这是第二条" }
    ]
  },
  "timestamp": 1759051234000
}


总结
可以做到通用 CRUD 接口 + JSON 返回，只要：
数据库差异封装在 Provider 层，接口层不关心数据库类型。
字段类型统一映射成 .NET 标准类型（int, long, string, bool, DateTime 等）。
JSON 返回统一，前端无需关心数据库字段类型。
配置文件驱动模块表名和字段，接口代码不需要改动即可切换数据库。
换句话说，只要 Provider 层做好“类型转换”和“SQL/Mongo 命令生成”，接口层完全可以 通用。


RESTful 风格（资源式接口）
例子：
GET    /api/news         → list
GET    /api/news/1       → get
POST   /api/news         → add
PUT    /api/news/1       → update
DELETE /api/news/1       → delete

优点
完全遵循 RESTful 规范，和主流框架/前端习惯一致。
接口语义清晰，调试和文档自动化（Swagger）更好用。
前端调用语义直观，不需要 action 字段。


如果你们团队更熟悉 RESTful 风格 → 用 list/get/add/update/delete
如果你们希望严格对齐 CRUD 术语 → 用 create/read/update/delete

最新读取

{
  "module": "company",
  "action": "read",
  "data": {
    "fields": ["ID", "AboutName", "Content"],
    "filter": {
        "ID": 3,
        "AboutName": "联系我们"
    },  
    "sort": [
      { "field": "AddTime", "direction": "DESC" },
      { "field": "ID", "direction": "ASC" }
    ],
    "page": 1,
    "pageSize": 10
  }
}


返回

{
  "success": true,
  "code": 0,
  "message": "操作成功",
  "data": {
    "count": 10,
    "total": 57,
    "page": 1,
    "pageSize": 10,
    "records": [
      {
        "ID": 3,
        "AboutName": "联系我们",
        "Content": "这是第一条"
      },
      {
        "ID": 4,
        "AboutName": "公司地址",
        "Content": "这是第二条"
      }
    ]
  },
  "timestamp": 1759051234
}


插入

{
  "module": "company",
  "action": "create",
  "data": {
    "AboutName": "测试公司",
    "ViewFlag": 0,
    "Content": "公司简介备注信息",
    "ClickNumber": 0,
    "Sequence": 1,
    "GroupID": "200603281858588888",
    "Exclusive": ">=",
    "NavType": 1,
    "ChildFlag": 0,
    "AddTime": "2025-09-24 10:30:00",
    "UpdateTime": "2025-09-24 10:30:00"
  }
}

更新

{
  "module": "company",
  "action": "update",
  "data": {
    "id": 4,
    "AboutName": "测试公司更新",
    "ViewFlag": 1,
    "Content": "公司简介备注信息",
    "ClickNumber": 30,
    "Sequence": 99,
    "GroupID": "200603281858588888",
    "Exclusive": ">=",
    "NavType": 1,
    "ChildFlag": 0,
    "AddTime": "2025-09-24 10:30:00",
    "UpdateTime": "2025-09-24 10:30:00"
  }
}


删除

{
  "module": "company",
  "action": "delete",
  "data": {
    "id": 4
  }
}


清缓存
{
  "module": "system",
  "action": "reloadConfig",
  "data": {}
}




17:10 2025/9/28

核心思想
接口只专注 CRUD 和 JSON 返回
数据库类型和连接由配置文件控制
前端无需关心数据库类型，只处理接口返回的 JSON
新增数据库或切换数据库类型，只需修改配置文件，无需改接口代码

配置文件示例（JSON）
{
  "CurrentDb": "SqlServer",
  "databases": {
    "SqlServer": {
      "type": "SqlServer",
      "connectionString": "Server=.;Database=MyDb;User Id=sa;Password=123;"
    },
    "MySql": {
      "type": "MySql",
      "connectionString": "Server=127.0.0.1;Database=NewsDb;Uid=root;Pwd=123456;"
    },
    "Mongo": {
      "type": "MongoDB",
      "connectionString": "mongodb://127.0.0.1:27017"
    }
  },
  "modules": {
    "company": {
      "table": "ZGBao_About",
      "allowedFields": ["AboutName","ViewFlag","Content","ClickNumber","Sequence","GroupID","Exclusive","NavType","ChildFlag","AddTime","UpdateTime"],
      "requiredFields": ["AboutName","Content"]
    },
    "news": {
      "table": "News",
      "allowedFields": ["title","content","category_id","status","author"],
      "requiredFields": ["title","content"]
    }
  }
}
CurrentDb：定义当前接口使用的数据库类型
databases：每种数据库的类型和连接信息
modules：模块只关心表名和字段，不包含数据库信息

接口实现逻辑
读取配置文件，解析 CurrentDb、databases 和 modules
接口接收请求（JSON：module、action、data）
根据 CurrentDb 获取对应数据库配置
调用数据库提供者执行 CRUD


优势

安全：接口统一管理
易维护：切换数据库只需改 CurrentDb 或对应数据库连接
可扩展：新增数据库类型只需在配置文件和数据库提供者工厂添加即可
前端独立：所有数据通过 JSON 渲染，前端不关心数据库细节


09:10 2025/9/27

黙认运行SQLServer Express需要
SQL Server 网络配置 SQLEXPRESS 的协议   TCP/IP (启用)  指定IP：1433，重启SQL Server

在 SSMS 中启用混合模式，这样SQL用户才可以登录
服务器实例 → 选择 属性（Properties）安全性（Security）服务器身份验证（Server authentication） 区域：选择 SQL Server 和 Windows 身份验证模式（Mixed Mode）

在 SQL Server 中，为了实现 “一个用户只管理特定数据库，隔离其他数据库”，正确的顺序是
建议顺序
1、先创建数据库，因为数据库用户需要映射到某个数据库，如果数据库不存在，创建用户就无法指定目标数据库
2、再创建 SQL Server 登录（Login）这是实例级别的账号，可以在 Mixed Mode 下使用 SQL 登录或 Windows 登录，在数据库中创建用户（User）并映射登录，给数据库用户赋权限（例如 db_owner）这个权限只在指定数据库有效，不影响其他数据库
3、用户映射创建的数据库：Navicat-> 右键编辑数据库 -> 标签：常规 -> 所有者：选中用户

正确执行示例（T-SQL）
-- 1. 创建数据库
CREATE DATABASE MyAppDB;
GO

-- 2. 创建 SQL 登录
CREATE LOGIN AppUser WITH PASSWORD = 'StrongPass!23';
GO

-- 3. 在数据库中创建用户并赋权限
USE MyAppDB;
GO
CREATE USER AppUser FOR LOGIN AppUser;
ALTER ROLE db_owner ADD MEMBER AppUser;
GO


执行完毕后：
AppUser 只能管理 MyAppDB
对其他数据库没有任何访问或管理权限


重点★★★★★（用户映射创建的数据库：Navicat-> 超级用户右键编辑数据库 -> 标签常规 -> 所有者选中用户）
GUI（Navicat/SSMS）操作顺序
创建数据库 → 右键 Databases → New Database
创建 SQL 登录 → Security → Logins → New Login
设置默认数据库为刚创建的数据库
用户映射 → 勾选目标数据库，并赋 db_owner

先创建数据库 → 再创建登录 → 再在数据库中创建用户并映射登录
这样才能保证用户隔离，只能管理指定数据库，符合生产环境安全最佳实践。

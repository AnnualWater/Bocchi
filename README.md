# Bocchi - - - - 测试版V0.0.2

### PostgreSQL

默认使用PostgreSQL数据库 \
如需切换数据库请参照[abp数据库迁移指南](https://docs.abp.io/en/abp/latest/Entity-Framework-Core-Other-DBMS)

### 数据迁移

1. 需要在 src/Bocchi.EntityFrameworkCore 执行 \
   注意此命令需要全局安装`dotnet-ef`

   > dotnet ef migrations add $MigrateName$ \
   > dotnet ef database update

2. 修改 src/Bocchi.DbMigrator和 src/Bocchi.Blazor 修改appsetting.json中的数据库连接字符串
3. 然后运行 src/Bocchi.DbMigrator项目执行数据库迁移

### 运行项目

1. 修改src/Bocchi.Blazor的appsettings.json文件，配置Sora相关设置
2. 运行项目src/Bocchi.Blazor
3. 修改go-cqhttp配置文件，修改上报类型为`array`
4. 运行go-cqhttp

### 当前已实现的功能

1. 自动获取`cpolar`穿透网址
2. 点歌插件（163，qq）
3. 番剧订阅（[樱花](https://www.yhpdm.com)）
4. 自动同意好友请求
5. 无密码登录

### 想要添加的功能

1. 雀魂比赛管理
2. 其他功能（咕咕咕——）
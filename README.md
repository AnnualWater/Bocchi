# Bocchi
## 测试版V0.0.1
### MSSQLServer
需要MSSQLServer数据库

### 数据迁移
1. 需要在 src/Bocchi.EntityFrameworkCore 执行 \
注意此命令需要安装dotnet-ef
> dotnet ef migrations add [MigrateName]
2. 修改 src/Bocchi.DbMigrator和 src/Bocchi.Blazor 修改appsetting.json中的数据库连接字符串 \
3. 然后运行 src/Bocchi.DbMigrator项目执行数据库迁移

### 运行项目
1. 修改src/Bocchi.Blazor的appsetting.json文件，配置Sora相关设置
2. 运行项目src/Bocchi.Blazor
3. 修改go-cqhttp配置文件，修改上报类型为array
4. 运行go-cqhttp

### 想要添加的功能
1. 使用cpolar穿透，自动获取穿透网址
2. 自定义问答插件
3. 其他功能（咕咕咕——）
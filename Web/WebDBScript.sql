Create Table [User](
Id int Primary Key Identity(1, 1),
UserName nvarchar(255) not null unique,
[Password] nvarchar(255) not null,
[PasswordHash] nvarchar(255) not null,
Email nvarchar(255) not null unique,
Active bit);

Create Table [Admin](
Id int Primary Key,
FullName nvarchar(255));

Create Table [Customer](
Id int Primary Key,
FullName nvarchar(255),
[Address] nvarchar(255),
Gender bit,
PhoneNumber nvarchar(255),
DateofBirth datetime);

Create Table [Product](
Id int Primary Key Identity(1, 1),
ProductId nvarchar(255) not null,
Title nvarchar(255) not null,
[Type] int not null,
[Price] float not null,
[Discount] float,
Content text,
Active bit);

Create Table [Category](
Id int Primary Key Identity(1, 1),
Title nvarchar(255) not null,
Active bit);

Create Table [ProductCategory](
ProductId int,
CategoryId int);

Create Table [ProductReview](
Id int Primary Key Identity(1, 1),
ProductId int not null,
Title nvarchar(255) not null,
Rating int not null,
PublishedAt datetime not null,
Content text);

Create Table [Cart](
Id int Primary Key Identity(1, 1),
UserId int not null);

Create Table [CartItem](
Id int Primary Key Identity(1, 1),
ProductId int not null,
CartId int not null,
Price float not null,
Discount float not null,
Quantity int not null);

Create Table [Order](
Id int Primary Key Identity(1, 1),
UserId int not null,
[Status] int not null,
Shipping float not null,
Total float not null,
Discount float not null,
GrandTotal float not null);

Create Table [OrderItem](
Id int Primary Key Identity(1, 1),
ProductId int not null,
OrderId int not null,
Price float not null,
Discount float not null,
Quantity int not null);

Alter Table [Admin] with nocheck
	Add Foreign Key (Id) References [User](Id)
Alter Table [Customer] with nocheck
	Add Foreign Key (Id) References [User](Id)
Alter Table [Cart] with nocheck
	Add Foreign Key (UserId) References [User](Id)
Alter Table [CartItem] with nocheck
	Add Foreign Key (ProductId) References [Product](Id)
Alter Table [CartItem] with nocheck
	Add Foreign Key (CartId) References [Cart](Id)
Alter Table [Order] with nocheck
	Add Foreign Key (UserId) References [User](Id)
Alter Table [OrderItem] with nocheck
	Add Foreign Key (ProductId) References [Product](Id)
Alter Table [OrderItem] with nocheck
	Add Foreign Key (OrderId) References [Order](Id)
Alter Table [ProductCategory] with nocheck
	Add Foreign Key (ProductId) References [Product](Id)
Alter Table [ProductCategory] with nocheck
	Add Foreign Key (CategoryId) References [Category](Id)
Alter Table [ProductReview] with nocheck
	Add Foreign Key (ProductId) References [Product](Id)
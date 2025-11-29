-- Tạo bảng UserPasswords để lưu password đã encrypt
-- LƯU Ý: Đây chỉ phù hợp cho môi trường nội bộ, không nên dùng trong production!

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'UserPasswords')
BEGIN
    CREATE TABLE [dbo].[UserPasswords] (
        [UserId] NVARCHAR(450) NOT NULL,
        [EncryptedPassword] NVARCHAR(MAX) NOT NULL,
        [LastUpdated] DATETIME2 NOT NULL,
        CONSTRAINT [PK_UserPasswords] PRIMARY KEY CLUSTERED ([UserId])
    );

    CREATE NONCLUSTERED INDEX [IX_UserPasswords_UserId] ON [dbo].[UserPasswords]([UserId]);

    -- Foreign key constraint
    ALTER TABLE [dbo].[UserPasswords]
    ADD CONSTRAINT [FK_UserPasswords_AspNetUsers_UserId]
        FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers]([Id])
        ON DELETE CASCADE;

    PRINT 'Bảng UserPasswords đã được tạo thành công!';
END
ELSE
BEGIN
    PRINT 'Bảng UserPasswords đã tồn tại!';
END



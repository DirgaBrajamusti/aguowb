/* Up Script*/
SET IDENTITY_INSERT [dbo].Apii
ON

INSERT INTO Config 
(
    ApiId,
    MenuId,
    ApiName,
    Url
)
VALUES
(
    273,
    18,
    'API Read',
    '/api/hauling/summaryhaulingrequest',
)
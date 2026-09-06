-- Access control seed: Roles, Permissions, RolePermissions
-- Idempotent: uses ON CONFLICT DO NOTHING (requires unique constraints on Roles.Name, Permissions.Key and unique(RoleId, PermissionId))

-- Roles
INSERT INTO "Roles" ("Id","Name","Description","IsActive","CreatedOnUtc", "ModifiedOnUtc")
VALUES
  ('11111111-1111-4111-8111-111111111111','Admin','Administrator with all permissions',true,now(),now()),
  ('22222222-2222-4222-8222-222222222222','Owner','Apartment owner with management permissions',true,now(),now()),
  ('33333333-3333-4333-8333-333333333333','Guest','Guest user with limited read access',true,now(),now())
ON CONFLICT ("Name") DO NOTHING;

-- Permissions
INSERT INTO "Permissions" ("Id","Key","Name","Description","Feature","Action","IsActive","SortOrder","CreatedOnUtc", "ModifiedOnUtc")
VALUES
  ('a1a1a1a1-0000-4000-8000-aaaaaaaaaaa1','apartments.read','Apartments: Read','Can read apartment listings','apartments','read',true,10,now(),now()),
  ('a1a1a1a1-0000-4000-8000-aaaaaaaaaaa2','apartments.create','Apartments: Create','Can create apartments','apartments','create',true,20,now(),now()),
  ('a1a1a1a1-0000-4000-8000-aaaaaaaaaaa3','apartments.update','Apartments: Update','Can update apartments','apartments','update',true,30,now(),now()),
  ('a1a1a1a1-0000-4000-8000-aaaaaaaaaaa4','apartments.delete','Apartments: Delete','Can delete apartments','apartments','delete',true,40,now(),now()),

  ('b1b1b1b1-0000-4000-8000-bbbbbbbbbbb1','bookings.create','Bookings: Create','Can create bookings','bookings','create',true,50,now(),now()),
  ('b1b1b1b1-0000-4000-8000-bbbbbbbbbbb2','bookings.read','Bookings: Read','Can read bookings','bookings','read',true,60,now(),now()),
  ('b1b1b1b1-0000-4000-8000-bbbbbbbbbbb3','bookings.cancel','Bookings: Cancel','Can cancel bookings','bookings','cancel',true,70,now(),now()),
  ('b1b1b1b1-0000-4000-8000-bbbbbbbbbbb4','bookings.manage','Bookings: Manage','Can fully manage bookings','bookings','manage',true,80,now(),now()),

  ('c1c1c1c1-0000-4000-8000-ccccccccccc1','reviews.create','Reviews: Create','Can create reviews','reviews','create',true,90,now(),now()),
  ('c1c1c1c1-0000-4000-8000-ccccccccccc2','reviews.update','Reviews: Update','Can update reviews','reviews','update',true,100,now(),now()),
  ('c1c1c1c1-0000-4000-8000-ccccccccccc3','reviews.delete','Reviews: Delete','Can delete reviews','reviews','delete',true,110,now(),now()),

  ('d1d1d1d1-0000-4000-8000-ddddddddddd1','users.manage-roles','Users: Manage Roles','Can manage user roles','users','manage-roles',true,120,now(),now()
)
ON CONFLICT ("Key") DO NOTHING;

-- Role -> Permission mappings
-- Admin: all permissions
INSERT INTO "RolePermissions" ("RoleId","PermissionId","CreatedOnUtc", "ModifiedOnUtc")
SELECT r."Id", p."Id", now(), now()
FROM "Roles" r
CROSS JOIN "Permissions" p
WHERE r."Name" = 'Admin'
ON CONFLICT ("RoleId","PermissionId") DO NOTHING;

-- Owner: apartments.read/create/update, bookings.create/read/cancel, reviews.create/update
INSERT INTO "RolePermissions" ("RoleId","PermissionId","CreatedOnUtc", "ModifiedOnUtc")
SELECT r."Id", p."Id", now(), now()
FROM "Roles" r
JOIN "Permissions" p ON p."Key" IN (
  'apartments.read','apartments.create','apartments.update',
  'bookings.create','bookings.read','bookings.cancel',
  'reviews.create','reviews.update'
)
WHERE r."Name" = 'Owner'
ON CONFLICT ("RoleId","PermissionId") DO NOTHING;

-- Guest: apartments.read, bookings.read
INSERT INTO "RolePermissions" ("RoleId","PermissionId","CreatedOnUtc", "ModifiedOnUtc")
SELECT r."Id", p."Id", now(), now()
FROM "Roles" r
JOIN "Permissions" p ON p."Key" IN ('apartments.read','bookings.read')
WHERE r."Name" = 'Guest'
ON CONFLICT ("RoleId","PermissionId") DO NOTHING;

-- Optional: quick verification counts
SELECT
  (SELECT COUNT(*) FROM "Roles") AS roles_count,
  (SELECT COUNT(*) FROM "Permissions") AS permissions_count,
  (SELECT COUNT(*) FROM "RolePermissions") AS role_permissions_count;

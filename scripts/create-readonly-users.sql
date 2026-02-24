-- Create read-only PostgreSQL users for MeilisearchSyncService
-- Run each section on the respective service's PostgreSQL instance

-- For event_db: kubectl exec -n event-service event-postgres-0 -- psql -U event -d event_db
CREATE USER meili_readonly WITH PASSWORD 'CHANGE_ME_EVENT';
GRANT CONNECT ON DATABASE event_db TO meili_readonly;
GRANT USAGE ON SCHEMA public TO meili_readonly;
GRANT SELECT ON TABLE public.mt_doc_event TO meili_readonly;

-- For venue_db: kubectl exec -n venue-service venue-postgres-0 -- psql -U venue -d venue_db
CREATE USER meili_readonly WITH PASSWORD 'CHANGE_ME_VENUE';
GRANT CONNECT ON DATABASE venue_db TO meili_readonly;
GRANT USAGE ON SCHEMA public TO meili_readonly;
GRANT SELECT ON TABLE public.mt_doc_venue TO meili_readonly;

-- For category_db: kubectl exec -n category-service category-postgres-0 -- psql -U category -d category_db
CREATE USER meili_readonly WITH PASSWORD 'CHANGE_ME_CATEGORY';
GRANT CONNECT ON DATABASE category_db TO meili_readonly;
GRANT USAGE ON SCHEMA public TO meili_readonly;
GRANT SELECT ON TABLE public.mt_doc_category TO meili_readonly;

-- For organizer_db: kubectl exec -n organizer-service organizer-postgres-0 -- psql -U organizer -d organizer_db
CREATE USER meili_readonly WITH PASSWORD 'CHANGE_ME_ORGANIZER';
GRANT CONNECT ON DATABASE organizer_db TO meili_readonly;
GRANT USAGE ON SCHEMA public TO meili_readonly;
GRANT SELECT ON TABLE public.mt_doc_organizer TO meili_readonly;

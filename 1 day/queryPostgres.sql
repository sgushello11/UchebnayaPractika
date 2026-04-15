--
-- PostgreSQL database dump
--

\restrict bC7yhxhLUbgkqeH32oeMt4UfSxn1JdD6QLpucYm3TMhvU6cP2sX0ZCyKEuM9W1S

-- Dumped from database version 18.3
-- Dumped by pg_dump version 18.2

-- Started on 2026-04-15 12:38:01

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET transaction_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

--
-- TOC entry 4 (class 2615 OID 2200)
-- Name: public; Type: SCHEMA; Schema: -; Owner: pg_database_owner
--

CREATE SCHEMA public;


ALTER SCHEMA public OWNER TO pg_database_owner;

--
-- TOC entry 5080 (class 0 OID 0)
-- Dependencies: 4
-- Name: SCHEMA public; Type: COMMENT; Schema: -; Owner: pg_database_owner
--

COMMENT ON SCHEMA public IS 'standard public schema';


SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- TOC entry 224 (class 1259 OID 16491)
-- Name: attachedfiles; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.attachedfiles (
    fileid integer NOT NULL,
    requestid integer NOT NULL,
    filetype character varying(20) NOT NULL,
    filepath character varying(255) NOT NULL,
    filename character varying(255) NOT NULL,
    uploadedat timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT attachedfiles_filetype_check CHECK (((filetype)::text = ANY ((ARRAY['passport_scan'::character varying, 'photo'::character varying])::text[])))
);


ALTER TABLE public.attachedfiles OWNER TO postgres;

--
-- TOC entry 223 (class 1259 OID 16490)
-- Name: attachedfiles_fileid_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.attachedfiles_fileid_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.attachedfiles_fileid_seq OWNER TO postgres;

--
-- TOC entry 5081 (class 0 OID 0)
-- Dependencies: 223
-- Name: attachedfiles_fileid_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.attachedfiles_fileid_seq OWNED BY public.attachedfiles.fileid;


--
-- TOC entry 228 (class 1259 OID 16555)
-- Name: employees; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.employees (
    employeeid integer NOT NULL,
    lastname character varying(50) NOT NULL,
    firstname character varying(50) NOT NULL,
    patronymic character varying(50),
    department character varying(100),
    division character varying(100),
    employeecode character varying(20)
);


ALTER TABLE public.employees OWNER TO postgres;

--
-- TOC entry 227 (class 1259 OID 16554)
-- Name: employees_employeeid_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.employees_employeeid_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.employees_employeeid_seq OWNER TO postgres;

--
-- TOC entry 5082 (class 0 OID 0)
-- Dependencies: 227
-- Name: employees_employeeid_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.employees_employeeid_seq OWNED BY public.employees.employeeid;


--
-- TOC entry 230 (class 1259 OID 16567)
-- Name: groupmembers; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.groupmembers (
    groupmemberid integer NOT NULL,
    groupvisitid integer NOT NULL,
    lastname character varying(50) NOT NULL,
    firstname character varying(50) NOT NULL,
    patronymic character varying(50),
    phone character varying(20),
    email character varying(100),
    passportdata character varying(11) NOT NULL,
    birthdate date NOT NULL,
    photopath character varying(255)
);


ALTER TABLE public.groupmembers OWNER TO postgres;

--
-- TOC entry 229 (class 1259 OID 16566)
-- Name: groupmembers_groupmemberid_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.groupmembers_groupmemberid_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.groupmembers_groupmemberid_seq OWNER TO postgres;

--
-- TOC entry 5083 (class 0 OID 0)
-- Dependencies: 229
-- Name: groupmembers_groupmemberid_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.groupmembers_groupmemberid_seq OWNED BY public.groupmembers.groupmemberid;


--
-- TOC entry 222 (class 1259 OID 16454)
-- Name: groupvisits; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.groupvisits (
    groupvisitid integer NOT NULL,
    requestid integer NOT NULL,
    excelfilepath character varying(255),
    templatefilepath character varying(255)
);


ALTER TABLE public.groupvisits OWNER TO postgres;

--
-- TOC entry 221 (class 1259 OID 16453)
-- Name: groupvisits_groupvisitid_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.groupvisits_groupvisitid_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.groupvisits_groupvisitid_seq OWNER TO postgres;

--
-- TOC entry 5084 (class 0 OID 0)
-- Dependencies: 221
-- Name: groupvisits_groupvisitid_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.groupvisits_groupvisitid_seq OWNED BY public.groupvisits.groupvisitid;


--
-- TOC entry 226 (class 1259 OID 16533)
-- Name: users; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.users (
    userid integer NOT NULL,
    lastname character varying(50) NOT NULL,
    firstname character varying(50) NOT NULL,
    patronymic character varying(50),
    phone character varying(20),
    email character varying(100) NOT NULL,
    birthdate date NOT NULL,
    passportdata character varying(11) NOT NULL,
    login character varying(50) NOT NULL,
    passwordhash character varying(255) NOT NULL,
    assignment character varying(50)
);


ALTER TABLE public.users OWNER TO postgres;

--
-- TOC entry 225 (class 1259 OID 16532)
-- Name: users_userid_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.users_userid_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.users_userid_seq OWNER TO postgres;

--
-- TOC entry 5085 (class 0 OID 0)
-- Dependencies: 225
-- Name: users_userid_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.users_userid_seq OWNED BY public.users.userid;


--
-- TOC entry 220 (class 1259 OID 16421)
-- Name: visitrequests; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.visitrequests (
    requestid integer NOT NULL,
    userid integer NOT NULL,
    requesttype character varying(20) NOT NULL,
    status character varying(20) DEFAULT 'проверка'::character varying NOT NULL,
    rejectionreason text,
    startdate date NOT NULL,
    enddate date NOT NULL,
    visitpurpose text NOT NULL,
    targetdepartment character varying(100) NOT NULL,
    targetemployeeid integer NOT NULL,
    note text NOT NULL,
    createdat timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT visitrequests_requesttype_check CHECK (((requesttype)::text = ANY ((ARRAY['личная'::character varying, 'групповая'::character varying])::text[]))),
    CONSTRAINT visitrequests_status_check CHECK (((status)::text = ANY ((ARRAY['проверка'::character varying, 'одобрена'::character varying, 'не одобрена'::character varying])::text[])))
);


ALTER TABLE public.visitrequests OWNER TO postgres;

--
-- TOC entry 219 (class 1259 OID 16420)
-- Name: visitrequests_requestid_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.visitrequests_requestid_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.visitrequests_requestid_seq OWNER TO postgres;

--
-- TOC entry 5086 (class 0 OID 0)
-- Dependencies: 219
-- Name: visitrequests_requestid_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.visitrequests_requestid_seq OWNED BY public.visitrequests.requestid;


--
-- TOC entry 4885 (class 2604 OID 16494)
-- Name: attachedfiles fileid; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.attachedfiles ALTER COLUMN fileid SET DEFAULT nextval('public.attachedfiles_fileid_seq'::regclass);


--
-- TOC entry 4888 (class 2604 OID 16558)
-- Name: employees employeeid; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.employees ALTER COLUMN employeeid SET DEFAULT nextval('public.employees_employeeid_seq'::regclass);


--
-- TOC entry 4889 (class 2604 OID 16570)
-- Name: groupmembers groupmemberid; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.groupmembers ALTER COLUMN groupmemberid SET DEFAULT nextval('public.groupmembers_groupmemberid_seq'::regclass);


--
-- TOC entry 4884 (class 2604 OID 16457)
-- Name: groupvisits groupvisitid; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.groupvisits ALTER COLUMN groupvisitid SET DEFAULT nextval('public.groupvisits_groupvisitid_seq'::regclass);


--
-- TOC entry 4887 (class 2604 OID 16536)
-- Name: users userid; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users ALTER COLUMN userid SET DEFAULT nextval('public.users_userid_seq'::regclass);


--
-- TOC entry 4881 (class 2604 OID 16424)
-- Name: visitrequests requestid; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.visitrequests ALTER COLUMN requestid SET DEFAULT nextval('public.visitrequests_requestid_seq'::regclass);


--
-- TOC entry 5068 (class 0 OID 16491)
-- Dependencies: 224
-- Data for Name: attachedfiles; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.attachedfiles (fileid, requestid, filetype, filepath, filename, uploadedat) FROM stdin;
1	1	passport_scan	/uploads/passports/passport_1.pdf	passport_ivanov.pdf	2026-04-15 12:18:27.17161
2	1	photo	/uploads/photos/photo_1.jpg	ivanov_photo.jpg	2026-04-15 12:18:27.17161
3	2	passport_scan	/uploads/passports/passport_2.pdf	passport_petrova.pdf	2026-04-15 12:18:27.17161
4	1	passport_scan	/uploads/passport_1.pdf	passport_ivanov.pdf	2026-04-15 12:22:21.206361
5	1	photo	/uploads/photo_1.jpg	ivanov_photo.jpg	2026-04-15 12:22:21.206361
6	2	passport_scan	/uploads/passport_2.pdf	passport_petrova.pdf	2026-04-15 12:22:21.206361
\.


--
-- TOC entry 5072 (class 0 OID 16555)
-- Dependencies: 228
-- Data for Name: employees; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.employees (employeeid, lastname, firstname, patronymic, department, division, employeecode) FROM stdin;
1	Фомичева	Авдотья	Трофимовна	Производство	\N	9367788
2	Гаврилова	Римма	Ефимовна	Сбыт	\N	9788737
3	Носкова	Наталия	Прохоровна	Администрация	\N	9736379
\.


--
-- TOC entry 5074 (class 0 OID 16567)
-- Dependencies: 230
-- Data for Name: groupmembers; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.groupmembers (groupmemberid, groupvisitid, lastname, firstname, patronymic, phone, email, passportdata, birthdate, photopath) FROM stdin;
1	1	Тостов	Петр	Петрович	89847592848	tosy@gmail.com	1111222233	1990-01-01	/photos/test1.jpg
2	1	Помелов	Максим	Иванович	89024356576	pomel@gmail.com	2222333344	1992-02-02	/photos/test2.jpg
3	2	Носков	Дмитрий	Петрович	89067876567	nos@gmail.com	3333444455	1995-03-03	/photos/test3.jpg
\.


--
-- TOC entry 5066 (class 0 OID 16454)
-- Dependencies: 222
-- Data for Name: groupvisits; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.groupvisits (groupvisitid, requestid, excelfilepath, templatefilepath) FROM stdin;
1	1	/uploads/group_list_1.xlsx	/templates/template_group.xlsx
2	2	/uploads/group_list_2.xlsx	/templates/template_group.xlsx
3	3	/uploads/group_list_3.xlsx	/templates/template_group.xlsx
\.


--
-- TOC entry 5070 (class 0 OID 16533)
-- Dependencies: 226
-- Data for Name: users; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.users (userid, lastname, firstname, patronymic, phone, email, birthdate, passportdata, login, passwordhash, assignment) FROM stdin;
9	Кириллова	Гавриила	Яковна	86487004334	Gavriila68@msn.com\n	1978-05-25	9438379667\n	Gavriila68\n	x4K5WthEe8ua\n	27/04/2023_9367788\n
1	Степанова	Радинка	 Власовна	86132726062	Radinka100@yandeRadinka100@yandex.ru	1986-10-18	0208530509\n	Vlas86\n	b3uWS6#Thuvq\n	24/04/2023_9367788\n
2	Шилов	Прохор	Герасимович	86155947766	Prohor156@list.ru\n	1977-10-09	3036796488\n	Prohor156\n	zDdom}SIhWs?\n	24/04/2023_9367788\n
3	Елисеева	Альбина	Николаевна	86548647746	Aljbina33@lenta.ru\n	1971-10-08	5241213304\n	Aljbina33\n	Bu?BHCtwDFin\n	25/04/2023_9367788\n
4	Шарова	Клавдия	Макаровна	88225258240	Klavdiya113@live.com\n	1983-02-15	8143593309\n	Klavdiya113\n	FjC#hNIJori}\n	25/04/2023_9788737\n
5	Сидорова	Тамара	Григорьевна	83346927977	Tamara179@live.com\n	1980-07-22	8143905520\n	Tamara179\n	TJxVqMXrbesI\n	25/04/2023_9736379\n
6	Петухов	Тарас	Фадеевич	83762206251	Taras24@rambler.ru\n	1995-11-22	1609171096\n	Taras24\n	07m5yspn3K~K\n	26/04/2023_9367788\n
7	Родионов	Аркадий	Власович	84916961711	Arkadij123@inbox.ru\n	1991-01-05	3841642594\n	Arkadij123\n	vk2N7lxX}ck%\n	26/04/2023_9788737\n
8	Горшкова	Глафира	Валентиновна	85533433882	Glafira73@outlook.com\n	1993-08-11	9170402601\n	Glafira73\n	Zz8POQlP}M4~\n	26/04/2023_9736379\n
10	Овчинников	Кузьма	Ефимович	85628661527	Kuzjma124@yandex.ru\n	1992-04-26	0766647226\n	Kuzjma124\n	OsByQJ}vYznW\n	27/04/2023_9788737\n
11	Беляков	Роман	Викторович	85951965628	Roman89@gmail.com\n	1993-08-02	2411478305\n	Roman89\n	Xd?xP$2yICcG\n	27/04/2023_9736379\n
12	Лыткин	Алексей	Максимович	89943532952	Aleksej43@gmail.com\n	1991-06-07	2383259825\n	Aleksej43\n	~c%PlTY0?qgl\n	28/04/2023_9367788\n
13	Шубина	Надежда	Викторовна	87364886695	Nadezhda137@outlook.com\n	1996-03-07	8844708476\n	Nadezhda137\n	QQ~0q~rXHb?p\n	28/04/2023_9788737\n
14	Зиновьева	Бронислава	Викторовна	87785651218	Bronislava56@yahoo.com\n	1981-09-24	6736319423\n	Bronislava56\n	LO}xyC~1S4l6\n	28/04/2023_9736379\n
\.


--
-- TOC entry 5064 (class 0 OID 16421)
-- Dependencies: 220
-- Data for Name: visitrequests; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.visitrequests (requestid, userid, requesttype, status, rejectionreason, startdate, enddate, visitpurpose, targetdepartment, targetemployeeid, note, createdat) FROM stdin;
1	1	личная	одобрена	\N	2026-04-16	2026-04-17	Деловая встреча	Производство	1	Примечание к заявке 1	2026-04-15 12:17:46.309577
2	2	групповая	проверка	\N	2026-04-18	2026-04-20	Экскурсия	Сбыт	2	Примечание к заявке 2	2026-04-15 12:17:46.309577
3	3	личная	не одобрена	Не предоставлены все необходимые документы	2026-04-16	2026-04-16	Технический осмотр	Администрация	3	Примечание к заявке 3	2026-04-15 12:17:46.309577
\.


--
-- TOC entry 5087 (class 0 OID 0)
-- Dependencies: 223
-- Name: attachedfiles_fileid_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.attachedfiles_fileid_seq', 6, true);


--
-- TOC entry 5088 (class 0 OID 0)
-- Dependencies: 227
-- Name: employees_employeeid_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.employees_employeeid_seq', 3, true);


--
-- TOC entry 5089 (class 0 OID 0)
-- Dependencies: 229
-- Name: groupmembers_groupmemberid_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.groupmembers_groupmemberid_seq', 3, true);


--
-- TOC entry 5090 (class 0 OID 0)
-- Dependencies: 221
-- Name: groupvisits_groupvisitid_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.groupvisits_groupvisitid_seq', 5, true);


--
-- TOC entry 5091 (class 0 OID 0)
-- Dependencies: 225
-- Name: users_userid_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.users_userid_seq', 1, true);


--
-- TOC entry 5092 (class 0 OID 0)
-- Dependencies: 219
-- Name: visitrequests_requestid_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.visitrequests_requestid_seq', 3, true);


-- Completed on 2026-04-15 12:38:01

--
-- PostgreSQL database dump complete
--

\unrestrict bC7yhxhLUbgkqeH32oeMt4UfSxn1JdD6QLpucYm3TMhvU6cP2sX0ZCyKEuM9W1S


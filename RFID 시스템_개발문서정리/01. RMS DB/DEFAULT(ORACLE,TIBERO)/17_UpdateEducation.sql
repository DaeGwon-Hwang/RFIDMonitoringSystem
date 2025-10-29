
-- 교육청 설치시에만 수행하여야 합니다.



-- 1. 교육청용 코드 사용 
UPDATE TB_ZZCOMTYPECD SET USE_FLAG='1' WHERE COM_TYPE_CD IN ('RD65','RD66');

-- 졸업대장 사용 
UPDATE TB_ZZCOMCD SET USE_FLAG='1' WHERE COM_TYPE_CD='RD65' AND COM_CD='11';

-- 생활기록부 사용 
UPDATE TB_ZZCOMCD SET USE_FLAG='1' WHERE COM_TYPE_CD='RD66' AND COM_CD='21';

-- 인사카드 사용 
UPDATE TB_ZZCOMCD SET USE_FLAG='1' WHERE COM_TYPE_CD='RD66' AND COM_CD='22';



-- 2.교육청 관련 메뉴 사용 
UPDATE TB_STMENU SET USE_FLAG='1' WHERE MENU_ID IN ('18','62','63','202','203','204');


--교육청 설치시에만 ROLLBACK 을 COMMIT; 로 변경 
rollback;
--commit;

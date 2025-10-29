
-------------------------------------------------------------
-- 단위업무 자체관리기관에서만 수행하여야 합니다.

-- 1. '단위업무관리' 화면 : 사용
UPDATE TB_STMENU SET USE_FLAG='1' WHERE MENU_ID='320';

-- 2. '단위업무신청처리', '단위업무결과접수처리' 화면 : 미사용
UPDATE TB_STMENU SET USE_FLAG='0' WHERE MENU_ID='324';
UPDATE TB_STMENU SET USE_FLAG='0' WHERE MENU_ID='325';


--단위업무 자체관리기관에만 ROLLBACK 을 COMMIT; 로 변경 
rollback;
--commit;

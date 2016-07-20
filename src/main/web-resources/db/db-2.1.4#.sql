USE $MAIN$;
/*****************************************************************************/

-- CONFIG
UPDATE config SET value='Dear $(USERNAME),\nyou requested a password reset for your Terradue account.\nPlease follow this link to set a new password:\n$(LINK)' WHERE `name`='EmailSupportResetPasswordBody';
-- RESULT
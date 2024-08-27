  /*Up script. */
  ALTER TABLE [Scheme]
  DROP COLUMN [MinRepeatability],
  COLUMN [MaxRepeatability],
  COLUMN [Details];


  /*Down script. */
  ALTER TABLE [Scheme]
  ADD [MinRepeatability] [decimal](18, 2) NULL,
  [MaxRepeatability] [decimal](18, 2) NULL,
  [Details] [varchar](max) NULL;
# Last tests resume:

================================================================================
---- Global Information --------------------------------------------------------
> request count                                      61478 (OK=61445  KO=33    )
> min response time                                      2 (OK=2      KO=3     )
> max response time                                    605 (OK=605    KO=81    )
> mean response time                                     9 (OK=9      KO=26    )
> std deviation                                         16 (OK=16     KO=24    )
> response time 50th percentile                          4 (OK=4      KO=13    )
> response time 75th percentile                          8 (OK=8      KO=30    )
> response time 95th percentile                         34 (OK=34     KO=71    )
> response time 99th percentile                         63 (OK=63     KO=79    )
> mean requests/sec                                250.931 (OK=250.796 KO=0.135 )
---- Response Time Distribution ------------------------------------------------
> t < 800 ms                                         61445 (100%)
> 800 ms <= t < 1200 ms                                  0 (  0%)
> t >= 1200 ms                                           0 (  0%)
> failed                                                33 (  0%)
---- Errors --------------------------------------------------------------------
> status.find.in(422), but actually found 200                        15 (45.45%)
> status.find.in(422), but actually found 400                        10 (30.30%)
> jmesPath(ultimas_transacoes[0].descricao).find.is(devolve), bu      5 (15.15%)
t actually found nothing
> jmesPath(saldo.total).find.is(0), but actually found -3             2 ( 6.06%)
> jmesPath(saldo.total).find.is(-25), but actually found -3           1 ( 3.03%)
================================================================================